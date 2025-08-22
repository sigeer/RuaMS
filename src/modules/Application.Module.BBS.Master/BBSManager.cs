using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.BBS.Common;
using Application.Module.BBS.Master.Models;
using Application.Utility;
using BBSProto;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Module.BBS.Master
{
    public class BBSManager : StorageBase<int, BBSThreadModel>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<BBSManager> _logger;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        int _threadId;

        public BBSManager(ILogger<BBSManager> logger, IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _threadId = (await dbContext.BbsThreads.MaxAsync(x => (int?)x.Threadid)) ?? 0;
        }

        public override List<BBSThreadModel> Query(Expression<Func<BBSThreadModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.BbsThreads.AsNoTracking().ProjectToType<BBSThreadModel>().Where(expression).ToList();
            var dbIdList = dbList.Select(x => x.Id).ToArray();

            var replies = dbContext.BbsReplies
                .Where(x => dbIdList.Contains(x.Replyid))
                .AsEnumerable()
                .GroupBy(x => x.Threadid)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dataFromDB = _mapper.Map<List<BBSThreadModel>>(dbList);

            foreach (var item in dataFromDB)
            {
                if (replies.TryGetValue(item.Id, out var eqList))
                {
                    item.Replies = _mapper.Map<List<BBSReplyModel>>(eqList);
                }
            }

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public BBSThreadModel? GetThread(int threadId)
        {
            return Query(x => x.Id == threadId).FirstOrDefault();
        }

        public BBSProto.ListBBSResponse ListBBSThreads(BBSProto.ListBBSRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new BBSProto.ListBBSResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };


            var total = Query(x => x.Guildid == chr.Character.GuildId).OrderByDescending(x => x.Timestamp).ToArray();

            var res = new BBSProto.ListBBSResponse();
            res.List.AddRange(_mapper.Map<BBSThreadPreviewDto[]>(total));
            return res;
        }

        public ShowBBSMainThreadResponse PostReply(BBSProto.PostReplyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var thread = GetThread(request.ThreadId);
            if (thread == null)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.ThreadNotFound };

            if (thread.Guildid != chr.Character.GuildId)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.GuildNotMatched };

            var replyId = thread.Replies.Count == 0 ? (thread.Id + 1) : (thread.Replies.Max(x => x.Id) + 1);
            if (replyId / 1000 != thread.Id)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.ExceedMaxReplyCount };

            var newModel = new BBSReplyModel
            {
                Content = request.Text,
                Postercid = request.MasterId,
                Threadid = request.ThreadId,
                Timestamp = _server.getCurrentTime(),
                Id = replyId
            };
            thread.Replies.Add(newModel);

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSThreadDto>(thread) };
        }

        public ShowBBSMainThreadResponse ShowThread(BBSProto.ShowThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var thread = GetThread(request.ThreadId);
            if (thread == null)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.ThreadNotFound };

            if (thread.Guildid != chr.Character.GuildId)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.GuildNotMatched };

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSThreadDto>(thread) };
        }

        public ShowBBSMainThreadResponse EditThread(BBSProto.PostThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var thread = GetThread(request.ThreadId);
            if (thread == null)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.ThreadNotFound };

            if (thread.Guildid != chr.Character.GuildId)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.GuildNotMatched };

            if (thread.Postercid != request.MasterId)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.NoAccess };

            thread.Name = request.Title;
            thread.Startpost = request.Text;
            thread.Timestamp = _server.getCurrentTime();
            thread.Icon = (short)request.Icon;

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSThreadDto>(thread) };
        }

        public ShowBBSMainThreadResponse PostThread(BBSProto.PostThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var newModel = new BBSThreadModel()
            {
                Postercid = request.MasterId,
                Timestamp = _server.getCurrentTime(),
                Name = request.Title,
                Icon = (short)request.Icon,
                Startpost = request.Text,
                Guildid = chr.Character.GuildId,
                Id = Interlocked.Add(ref _threadId, 1000),
            };
            SetDirty(newModel.Id, new StoreUnit<BBSThreadModel>(StoreFlag.AddOrUpdate, newModel));

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSThreadDto>(newModel) };
        }


        public void DeleteThread(BBSProto.DeleteThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            var thread = GetThread(request.ThreadId);
            if (thread == null)
            {
                return;
            }

            if (thread.Postercid != request.MasterId && chr.Character.GuildRank > 2)
            {
                return;
            }

            SetDirty(request.ThreadId, new StoreUnit<BBSThreadModel>(StoreFlag.Remove, null));
        }

        public ShowBBSMainThreadResponse DeleteBBSReply(BBSProto.DeleteReplyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var threadId = (request.ReplyId / 1000);
            var thread = GetThread(threadId);
            if (thread == null)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.ThreadNotFound };

            if (request.MasterId != thread.Postercid && chr.Character.GuildRank > 2)
                return new ShowBBSMainThreadResponse() { Code = (int)BBSResponseCode.NoAccess };

            thread.Replies.RemoveAll(x => x.Id == request.ReplyId);

            SetDirty(threadId, new StoreUnit<BBSThreadModel>(StoreFlag.AddOrUpdate, thread));

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSThreadDto>(thread) };
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<BBSThreadModel>> updateData)
        {
            var updateThreads = updateData.Keys.ToArray();
            await dbContext.BbsThreads.Where(x => updateThreads.Contains(x.Threadid)).ExecuteDeleteAsync();
            await dbContext.BbsReplies.Where(x => updateThreads.Contains(x.Threadid)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new BbsThreadEntity()
                    {
                        Threadid = obj.Id,
                        Guildid = obj.Guildid,
                        Icon = obj.Icon,
                        Name = obj.Name,
                        Postercid = obj.Postercid,
                        Startpost = obj.Startpost,
                        Timestamp = obj.Timestamp,
                    };
                    dbContext.BbsThreads.Add(dbData);
                    dbContext.BbsReplies.AddRange(obj.Replies.Select(x => new BbsReplyEntity(x.Id, x.Threadid, x.Postercid, x.Timestamp, x.Content)));
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}

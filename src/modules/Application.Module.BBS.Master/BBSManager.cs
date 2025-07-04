using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.BBS.Common;
using Application.Module.BBS.Master.Models;
using Application.Utility;
using AutoMapper;
using BBSProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Module.BBS.Master
{
    public class BBSManager : StorageBase<int, UpdateField<BBSThreadModel>>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<BBSManager> _logger;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        int _threadId;
        ConcurrentDictionary<int, BBSThreadModel> _threads = new();

        public BBSManager(ILogger<BBSManager> logger, IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public async Task Initialize(DBContext dbContext)
        {
            _threadId = (await dbContext.BbsThreads.MaxAsync(x => (int?)x.Threadid)) ?? 0;
        }

        public BBSThreadModel? GetThread(int threadId)
        {
            if (_threads.TryGetValue(threadId, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();

            var threadData = dbContext.BbsThreads.FirstOrDefault(x => x.Threadid == threadId);
            if (threadData == null)
                return null;

            var localData = _mapper.Map<BBSThreadModel>(threadData);
            var replies = dbContext.BbsReplies.Where(x => x.Threadid == threadId).ToList();

            localData.Replies = _mapper.Map<List<BBSReplyModel>>(replies);
            _threads[threadId] = localData;
            return localData;
        }

        public BBSProto.ListBBSResponse ListBBSThreads(BBSProto.ListBBSRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return new BBSProto.ListBBSResponse() { Code = (int)BBSResponseCode.CharacterNoGuild };

            var dataFromCache = _mapper.Map<List<BBSThreadPreview>>(_threads.Values.Where(x => x.Guildid == chr.Character.GuildId).ToList());

            var cachedid = dataFromCache.Select(x => x.ThreadId).ToArray();
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataFromDB = (from a in dbContext.BbsThreads.Where(x => !cachedid.Contains(x.Threadid) && x.Guildid == chr.Character.GuildId)
                              let replyCount = dbContext.BbsReplies.Count(x => x.Threadid == a.Threadid)
                              select new { thread = a, replyCount }).ToList()
                              .Select(x =>
                              {
                                  var r = _mapper.Map<BBSThreadPreview>(x.thread);
                                  r.ReplyCount = x.replyCount;
                                  return r;
                              }).ToList();

            var total = dataFromCache.Concat(dataFromDB).OrderByDescending(x => x.Timestamp).ToArray();

            var res = new BBSProto.ListBBSResponse();
            res.List.AddRange(total);
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

            var replyId = thread.Replies.Count == 0 ? (thread.ThreadId + 1) : (thread.Replies.Max(x => x.Id) + 1);
            if (replyId / 1000 != thread.ThreadId)
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

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSMainThread>(thread) };
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

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSMainThread>(thread) };
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

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSMainThread>(thread) };
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
                ThreadId = Interlocked.Add(ref _threadId, 1000),
            };
            _threads[newModel.ThreadId] = newModel;
            SetDirty(newModel.ThreadId, new UpdateField<BBSThreadModel>(UpdateMethod.AddOrUpdate, newModel));

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSMainThread>(newModel) };
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

            if (_threads.TryRemove(request.ThreadId, out var d))
            {
                SetDirty(request.ThreadId, new UpdateField<BBSThreadModel>(UpdateMethod.Remove, d));
            }
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

            SetDirty(threadId, new UpdateField<BBSThreadModel>(UpdateMethod.AddOrUpdate, thread));

            return new ShowBBSMainThreadResponse() { Data = _mapper.Map<BBSProto.BBSMainThread>(thread) };
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, UpdateField<BBSThreadModel>> updateData)
        {
            var updateThreads = updateData.Values.Select(x => x.Data.ThreadId).ToArray();
            await dbContext.BbsThreads.Where(x => updateThreads.Contains(x.Threadid)).ExecuteDeleteAsync();
            await dbContext.BbsReplies.Where(x => updateThreads.Contains(x.Threadid)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Method == UpdateMethod.AddOrUpdate)
                {
                    var dbData = new BbsThreadEntity()
                    {
                        Threadid = obj.ThreadId,
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

using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Module.BBS.Master.Models;
using Application.Utility;
using AutoMapper;
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
        readonly MasterTransport _transport;

        int _threadId;
        ConcurrentDictionary<int, BBSThreadModel> _threads = new();

        public BBSManager(ILogger<BBSManager> logger, IMapper mapper, MasterServer server, MasterTransport transport, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _server = server;
            _transport = transport;
            _dbContextFactory = dbContextFactory;
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

        public void ListBBSThreads(BBSProto.ListBBSRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
            {
                return;
            }

            var list = _mapper.Map<BBSProto.BBSThreadPreview[]>(_threads.Values);
            var res = new BBSProto.ListBBSResponse();
            res.List.AddRange(list);

            _transport.SendLoadThreadList(res);
        }

        public void PostReply(BBSProto.PostReplyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            if (!_threads.TryGetValue(request.ThreadId, out var thread))
            {
                return;
            }

            var replyId = thread.Replies.Count == 0 ? (thread.Localthreadid + 1) : (thread.Replies.Max(x => x.Id) + 1);
            var newModel = new BBSReplyModel
            {
                Content = request.Text,
                Postercid = request.MasterId,
                Threadid = request.ThreadId,
                Timestamp = _server.getCurrentTime(),
                Id = replyId
            };
            thread.Replies.Add(newModel);

            _transport.SendLoadThread(request.MasterId, _mapper.Map<BBSProto.BBSMainThread>(thread));
        }

        public void ShowThread(BBSProto.ShowThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            if (!_threads.TryGetValue(request.ThreadId, out var thread))
            {
                return;
            }

            if (thread.Guildid != chr.Character.GuildId)
                return;

            _transport.SendLoadThread(request.MasterId, _mapper.Map<BBSProto.BBSMainThread>(thread));
        }

        public void EditThread(BBSProto.PostThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            if (!_threads.TryGetValue(request.ThreadId, out var thread))
            {
                return;
            }

            thread.Name = request.Title;
            thread.Startpost = request.Text;
            thread.Timestamp = _server.getCurrentTime();
            thread.Icon = (short)request.Icon;

            _transport.SendLoadThread(request.MasterId, _mapper.Map<BBSProto.BBSMainThread>(thread));
        }

        public void PostThread(BBSProto.PostThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            var newModel = new BBSThreadModel()
            {
                Postercid = request.MasterId,
                Timestamp = _server.getCurrentTime(),
                Name = request.Title,
                Icon = (short)request.Icon,
                Startpost = request.Text,
                Guildid = chr.Character.GuildId,
                Localthreadid = Interlocked.Add(ref _threadId, 1000),
            };
            _threads[newModel.Localthreadid] = newModel;

            _transport.SendLoadThread(request.MasterId, _mapper.Map<BBSProto.BBSMainThread>(newModel));
        }


        public void DeleteThread(BBSProto.DeleteThreadRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            if (!_threads.TryGetValue(request.ThreadId, out var thread))
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

        public void deleteBBSReply(BBSProto.DeleteReplyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Character.GuildId <= 0)
                return;

            var threadId = (request.ReplyId / 1000);
            if (!_threads.TryGetValue(threadId, out var thread))
            {
                return;
            }

            if (request.MasterId != thread.Postercid && chr.Character.GuildRank > 2)
            {
                return;
            }

            thread.Replies.RemoveAll(x => x.Id == request.ReplyId);

            SetDirty(threadId, new UpdateField<BBSThreadModel>(UpdateMethod.AddOrUpdate, thread));

            _transport.SendLoadThread(request.MasterId, _mapper.Map<BBSProto.BBSMainThread>(thread));
        }

        protected override Task CommitInternal(DBContext dbContext, Dictionary<int, UpdateField<BBSThreadModel>> updateData)
        {
            return Task.CompletedTask;
        }
    }
}

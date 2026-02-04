using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Module.BBS.Channel.Net;
using Microsoft.Extensions.Logging;

namespace Application.Module.BBS.Channel
{
    public class BBSManager
    {
        readonly ILogger<BBSManager> _logger;
        readonly IChannelTransport _transport;
        readonly WorldChannelServer _server;

        public BBSManager(ILogger<BBSManager> logger, IChannelTransport transport, WorldChannelServer server)
        {
            _logger = logger;
            _transport = transport;
            _server = server;
        }

        public void listBBSThreads(Player chr, int start)
        {
            var res = _transport.ListThreads(new BBSProto.ListBBSRequest { MasterId = chr.Id });
            if (res.Code == 0)
            {
                chr.sendPacket(BBSPacketCreator.BBSThreadList(res.List.ToList(), start));
            }
        }


        public void PostReply(Player chr, int threadId, string text)
        {
            _transport.PostReply(new BBSProto.PostReplyRequest { MasterId = chr.Id, Text = text, ThreadId = threadId });
        }

        void ProcessThreadResponse(Player chr, BBSProto.ShowBBSMainThreadResponse data)
        {
            if (data.Code == 0)
                chr.sendPacket(BBSPacketCreator.showThread(data.Data));
            else
            {

            }
        }

        public void editBBSThread(Player chr, string title, string text, int icon, int localthreadid)
        {
            if (chr.getGuildId() < 1)
            {
                return;
            }

            var res = _transport.EditThread(new BBSProto.PostThreadRequest { Icon = icon, MasterId = chr.Id, Text = text, ThreadId = localthreadid, Title = title });
            ProcessThreadResponse(chr, res);
        }

        public void newBBSThread(Player chr, string title, string text, int icon, bool bNotice)
        {
            if (chr.GuildId <= 0)
            {
                return;
            }

            // TODO: 这里bNotice不明，重构改变了原逻辑（关键字段: localthreadid）
            var res = _transport.EditThread(new BBSProto.PostThreadRequest { Icon = icon, MasterId = chr.Id, Text = text, Title = title, BNotice = bNotice });
            ProcessThreadResponse(chr, res);
        }

        public void deleteBBSThread(Player chr, int localthreadid)
        {
            if (chr.getGuildId() <= 0)
            {
                return;
            }

            _transport.DeleteThread(new BBSProto.DeleteThreadRequest { MasterId = chr.Id, ThreadId = localthreadid });
        }

        public void deleteBBSReply(Player chr, int replyid)
        {
            if (chr.getGuildId() <= 0)
            {
                return;
            }

            var res = _transport.DeleteReply(new BBSProto.DeleteReplyRequest { MasterId = chr.Id, ReplyId = replyid });
            ProcessThreadResponse(chr, res);
        }

        internal void ShowThread(Player chr, int threadId)
        {
            var data = _transport.ShowThread(new BBSProto.ShowThreadRequest { MasterId = chr.Id, ThreadId = threadId });
            ProcessThreadResponse(chr, data);
        }
    }
}

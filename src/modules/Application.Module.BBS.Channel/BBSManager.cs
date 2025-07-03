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

        public void listBBSThreads(IPlayer chr, int start)
        {
            _transport.ListThreads(new BBSProto.ListBBSRequest { MasterId = chr.Id, Start = start });
        }

        public void OnThreadListReceived(BBSProto.ListBBSResponse data)
        {
            var chr = _server.FindPlayerById(data.Request.MasterId);
            if (chr != null)
                chr.sendPacket(BBSPacketCreator.BBSThreadList(data.List.ToList(), data.Request.Start));
        }


        public void PostReply(IPlayer chr, int threadId, string text)
        {
            _transport.PostReply(new BBSProto.PostReplyRequest { MasterId = chr.Id, Text = text, ThreadId = threadId });
        }

        public void OnBBSDataReceived(BBSProto.BBSMainThreadResponse data)
        {
            var chr = _server.FindPlayerById(data.ReceiverId);
            if (chr != null)
                chr.sendPacket(BBSPacketCreator.showThread(data.Data));
        }

        public void editBBSThread(IPlayer chr, string title, string text, int icon, int localthreadid)
        {
            if (chr.getGuildId() < 1)
            {
                return;
            }

            _transport.EditThread(new BBSProto.PostThreadRequest { Icon = icon, MasterId = chr.Id, Text = text, ThreadId = localthreadid, Title = title });
        }

        public void newBBSThread(IPlayer chr, string title, string text, int icon, bool bNotice)
        {
            if (chr.GuildId <= 0)
            {
                return;
            }

            _transport.EditThread(new BBSProto.PostThreadRequest { Icon = icon, MasterId = chr.Id, Text = text, Title = title });
        }

        public void deleteBBSThread(IPlayer chr, int localthreadid)
        {
            if (chr.getGuildId() <= 0)
            {
                return;
            }

            _transport.DeleteThread(new BBSProto.DeleteThreadRequest { MasterId = chr.Id, ThreadId = localthreadid });
        }

        public void deleteBBSReply(IPlayer chr, int replyid)
        {
            if (chr.getGuildId() <= 0)
            {
                return;
            }

            _transport.DeleteReply(new BBSProto.DeleteReplyRequest { MasterId = chr.Id, ReplyId = replyid });
        }

        internal void ShowThread(IPlayer chr, int threadId)
        {
            _transport.ShowThread(new BBSProto.ShowThreadRequest { MasterId = chr.Id, ThreadId = threadId });
        }
    }
}

using Application.Core.ServerTransports;
using Dto;

namespace Application.Core.Channel.Services
{
    public class ReportService
    {
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;

        public ReportService(IChannelServerTransport transport, WorldChannelServer server)
        {
            _transport = transport;
            _server = server;
        }

        public void SendReport(IPlayer chr, string victim, string text, int reason, string chatLog)
        {
            var res = _transport.SendReport(new SendReportRequest { MasterId = chr.Id, Victim = victim, Text = text, Reason = reason, ChatLog = chatLog });
            if (!res.IsSuccess)
            {
                chr.dropMessage($"玩家 {victim} 不存在");
            }
        }

        public void OnGMReceivedReport(SendReportBroadcast data)
        {
            foreach (var gmId in data.GmId)
            {
                var gmChr = _server.FindPlayerById(gmId);
                if (gmChr != null)
                {
                    gmChr.dropMessage(6, data.Text);
                }
            }
        }
    }
}

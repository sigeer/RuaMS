using Application.Core.ServerTransports;
using Dto;
using System.Threading.Tasks;

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

        public void SendReport(Player chr, string victim, string text, int reason, string chatLog)
        {
            _ = _transport.SendReport(new SendReportRequest { MasterId = chr.Id, Victim = victim, Text = text, Reason = reason, ChatLog = chatLog });
        }
    }
}

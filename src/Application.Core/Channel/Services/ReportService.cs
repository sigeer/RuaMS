using Application.Core.ServerTransports;

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

        public async Task SendReport(Player chr, string victim, string text, int reason, string chatLog)
        {
            await _transport.SendReport(new ProtoService.SendReportRequest { MasterId = chr.Id, Victim = victim, Text = text, Reason = reason, ChatLog = chatLog });
        }
    }
}

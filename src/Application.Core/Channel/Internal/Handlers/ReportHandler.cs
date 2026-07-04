using Application.Shared.Message;
using Dto;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class ReportHandler : InternalSessionChannelHandler<SendReportResponse>
    {
        public ReportHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.HandleReportReceived;

        protected override async Task HandleMessage(SendReportResponse res)
        {
            if (res.Code != 0)
            {
                await _server.SendToPlayerAsync(res.MasterId, async chr =>
                {
                    await chr.Pink($"玩家 {res.Victime} 不存在");
                });
            }
        }

        protected override SendReportResponse Parse(ByteString data) => SendReportResponse.Parser.ParseFrom(data);
    }
}

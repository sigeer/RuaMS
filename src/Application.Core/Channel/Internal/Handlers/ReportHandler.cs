using Application.Core.Channel.Commands;
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

        protected override void HandleMessage(SendReportResponse res)
        {
            if (res.Code != 0)
            {
                _server.PushChannelCommand(new InvokeDropMessageCommand(res.MasterId, 5, $"玩家 {res.Victime} 不存在"));
            }

        }

        protected override SendReportResponse Parse(ByteString data) => SendReportResponse.Parser.ParseFrom(data);
    }
}

using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class ReportHandler : InternalSessionChannelHandler<SendReportResponse>
    {
        public ReportHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.HandleReportReceived;

        protected override Task HandleAsync(SendReportResponse res, CancellationToken cancellationToken = default)
        {
            var chr = _server.FindPlayerById(res.MasterId);
            if (chr != null)
            {
                if (res.Code != 0)
                {
                    chr.dropMessage($"玩家 {res.Victime} 不存在");
                }
            }
            return Task.CompletedTask;

        }

        protected override SendReportResponse Parse(ByteString data) => SendReportResponse.Parser.ParseFrom(data);
    }
}

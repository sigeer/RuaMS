using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeCouponConfigUpdateHandler : InternalSessionChannelHandler<Config.CouponConfig>
    {
        public InvokeCouponConfigUpdateHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.OnCouponConfigUpdate;

        protected override Task HandleAsync(CouponConfig res, CancellationToken cancellationToken = default)
        {
            _server.UpdateCouponConfig(res);
            return Task.CompletedTask;
        }

        protected override CouponConfig Parse(ByteString data) => CouponConfig.Parser.ParseFrom(data);
    }
}

using Application.Core.Channel.Commands;
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

        public override int MessageId => (int)ChannelRecvCode.OnCouponConfigUpdate;

        protected override void HandleMessage(CouponConfig res)
        {
            _server.UpdateCouponConfig(res);
            _server.PushChannelCommand(new InvokeCouponConfigUpdateCommand(res));
        }

        protected override CouponConfig Parse(ByteString data) => CouponConfig.Parser.ParseFrom(data);
    }
}

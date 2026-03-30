using Application.Shared.Message;
using Config;
using Google.Protobuf;

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
            _server.Broadcast(w =>
            {
                w.getPlayerStorage().ProcessAllCharacters(chr =>
                {
                    chr.updateCouponRates();
                });
            });
        }

        protected override CouponConfig Parse(ByteString data) => CouponConfig.Parser.ParseFrom(data);
    }
}

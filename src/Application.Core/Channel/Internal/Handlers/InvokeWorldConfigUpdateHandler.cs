using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeWorldConfigUpdateHandler : InternalSessionChannelHandler<ProtoModel.WorldConfig>
    {
        public InvokeWorldConfigUpdateHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.OnWorldConfigUpdate;

        protected override Task HandleMessage(ProtoModel.WorldConfig res)
        {
            _server.UpdateWorldConfig(res);
            return _server.BroadcastAsync(async w =>
            {
                await w.UpdateWorldConfig(res);
            });
        }

        protected override ProtoModel.WorldConfig Parse(ByteString data) => ProtoModel.WorldConfig.Parser.ParseFrom(data);
    }
}

using Application.Shared.Message;
using Config;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeWorldConfigUpdateHandler : InternalSessionChannelHandler<WorldConfig>
    {
        public InvokeWorldConfigUpdateHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.OnWorldConfigUpdate;

        protected override Task HandleMessage(WorldConfig res)
        {
            _server.UpdateWorldConfig(res);
            return _server.BroadcastAsync(async w =>
            {
                await w.UpdateWorldConfig(res);
            });
        }

        protected override WorldConfig Parse(ByteString data) => WorldConfig.Parser.ParseFrom(data);
    }
}

using Application.Shared.Internal;
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

        protected override Task HandleAsync(WorldConfig res, CancellationToken cancellationToken = default)
        {
            _server.UpdateWorldConfig(res);
            return Task.CompletedTask;
        }

        protected override WorldConfig Parse(ByteString data) => WorldConfig.Parser.ParseFrom(data);
    }
}

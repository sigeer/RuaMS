using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class UnregisterChannelServerHandler : InternalSessionEmptyHandler
    {
        public UnregisterChannelServerHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.UnregisterChannel;

        protected override async Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            await _server.Shutdown();
        }

    }
}

using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class UnregisterChannelServerHandler : InternalSessionHandler<Empty>
    {
        public UnregisterChannelServerHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.UnregisterChannel;

        protected override async Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            await _server.Shutdown();
        }

        protected override Empty Parse(ByteString content) => Empty.Parser.ParseFrom(content);

    }
}

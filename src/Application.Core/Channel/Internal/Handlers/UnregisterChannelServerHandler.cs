using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class UnregisterChannelServerHandler : InternalSessionChannelEmptyHandler
    {
        public UnregisterChannelServerHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.UnregisterChannel;

        protected override void HandleMessage(Empty message)
        {
            _ = _server.Shutdown();
        }

    }
}

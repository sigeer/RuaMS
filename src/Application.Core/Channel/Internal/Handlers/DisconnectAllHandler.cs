using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionChannelEmptyHandler
    {
        public DisconnectAllHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DisconnectAll;

        protected override Task HandleMessage(Empty message)
        {
            return _server.BroadcastAsync(async w =>
            {
                await w.getPlayerStorage().disconnectAll(false);
            });
        }
    }
}

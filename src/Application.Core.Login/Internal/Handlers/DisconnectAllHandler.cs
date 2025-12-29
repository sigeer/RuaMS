using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionEmptyHandler<MasterServer>
    {
        public DisconnectAllHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => ChannelSendCode.DisconnectAll;

        protected override async Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
        }
    }
}

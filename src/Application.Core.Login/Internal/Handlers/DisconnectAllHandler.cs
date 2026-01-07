using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using SystemProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionMasterEmptyHandler
    {
        public DisconnectAllHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.DisconnectAll;

        protected override async Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
        }
    }

    internal class DisconnectOneHandler : InternalSessionMasterHandler<DisconnectPlayerByNameRequest>
    {
        public DisconnectOneHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.DisconnectOne;

        protected override async Task HandleAsync(DisconnectPlayerByNameRequest message, CancellationToken cancellationToken = default)
        {
            await _server.CrossServerService.DisconnectPlayerByName(message);
        }

        protected override DisconnectPlayerByNameRequest Parse(ByteString data)
        {
            return DisconnectPlayerByNameRequest.Parser.ParseFrom(data);
        }
    }
}

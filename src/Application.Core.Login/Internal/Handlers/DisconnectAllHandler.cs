using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionMasterEmptyHandler
    {
        public DisconnectAllHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.DisconnectAll;

        protected override Task HandleMessage(Empty message)
        {
            return _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
        }
    }

    internal class DisconnectOneHandler : InternalSessionMasterHandler<ProtoService.DisconnectPlayerByNameRequest>
    {
        public DisconnectOneHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.DisconnectOne;

        protected override Task HandleMessage(ProtoService.DisconnectPlayerByNameRequest message)
        {
            return _server.CrossServerService.DisconnectPlayerByName(message);
        }

        protected override ProtoService.DisconnectPlayerByNameRequest Parse(ByteString data)
        {
            return ProtoService.DisconnectPlayerByNameRequest.Parser.ParseFrom(data);
        }
    }
}

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

        protected override Task HandleMessage(Empty message)
        {
            return _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
        }
    }

    internal class DisconnectOneHandler : InternalSessionMasterHandler<DisconnectPlayerByNameRequest>
    {
        public DisconnectOneHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.DisconnectOne;

        protected override Task HandleMessage(DisconnectPlayerByNameRequest message)
        {
            return _server.CrossServerService.DisconnectPlayerByName(message);
        }

        protected override DisconnectPlayerByNameRequest Parse(ByteString data)
        {
            return DisconnectPlayerByNameRequest.Parser.ParseFrom(data);
        }
    }
}

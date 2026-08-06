using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BroadcastHandlers
    {
        internal class BroadcastWorldConfigUpdateHandler : InternalSessionMasterHandler<ProtoModel.WorldConfig>
        {
            public BroadcastWorldConfigUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateWorldConfig;

            protected override Task HandleMessage(ProtoModel.WorldConfig message)
            {
                return _server.UpdateWorldConfig(message);
            }

            protected override ProtoModel.WorldConfig Parse(ByteString content) => ProtoModel.WorldConfig.Parser.ParseFrom(content);
        }

        internal class BroadcastMessageHandler : InternalSessionMasterHandler<ProtoService.DropMessageRequest>
        {
            public BroadcastMessageHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropMessage;

            protected override Task HandleMessage(ProtoService.DropMessageRequest message)
            {
                return _server.DropWorldMessage(message.Type, message.Message, message.OnlyGM);
            }

            protected override ProtoService.DropMessageRequest Parse(ByteString content) => ProtoService.DropMessageRequest.Parser.ParseFrom(content);
        }

        internal class BroadcastPacketHandler : InternalSessionMasterHandler<ProtoService.PacketRequest>
        {
            public BroadcastPacketHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.BroadcastPacket;

            protected override Task HandleMessage(ProtoService.PacketRequest message)
            {
                return _server.BroadcastPacket(message);
            }

            protected override ProtoService.PacketRequest Parse(ByteString content) => ProtoService.PacketRequest.Parser.ParseFrom(content);
        }
    }
}

using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Google.Protobuf;
using MessageProto;
using Microsoft.AspNetCore.Hosting.Server;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BroadcastHandlers
    {
        internal class BroadcastWorldConfigUpdateHandler : InternalSessionMasterHandler<Config.WorldConfig>
        {
            public BroadcastWorldConfigUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateWorldConfig;

            protected override Task HandleMessage(WorldConfig message)
            {
                return _server.UpdateWorldConfig(message);
            }

            protected override WorldConfig Parse(ByteString content) => WorldConfig.Parser.ParseFrom(content);
        }

        internal class BroadcastMessageHandler : InternalSessionMasterHandler<DropMessageRequest>
        {
            public BroadcastMessageHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropMessage;

            protected override Task HandleMessage(DropMessageRequest message)
            {
                return _server.DropWorldMessage(message.Type, message.Message, message.OnlyGM);
            }

            protected override DropMessageRequest Parse(ByteString content) => DropMessageRequest.Parser.ParseFrom(content);
        }

        internal class BroadcastPacketHandler : InternalSessionMasterHandler<PacketRequest>
        {
            public BroadcastPacketHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.BroadcastPacket;

            protected override Task HandleMessage(PacketRequest message)
            {
                return _server.BroadcastPacket(message);
            }

            protected override PacketRequest Parse(ByteString content) => PacketRequest.Parser.ParseFrom(content);
        }
    }
}

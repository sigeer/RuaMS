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
        internal class BroadcastCouponConfigUpdateHandler : InternalSessionMasterHandler<Config.ToggelCouponRequest>
        {
            public BroadcastCouponConfigUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ToggleCoupon;

            protected override void HandleMessage(ToggelCouponRequest message)
            {
                _ = _server.CouponManager.ToggleCoupon(message.Id);
            }

            protected override ToggelCouponRequest Parse(ByteString content) => ToggelCouponRequest.Parser.ParseFrom(content);
        }
        internal class BroadcastWorldConfigUpdateHandler : InternalSessionMasterHandler<Config.WorldConfig>
        {
            public BroadcastWorldConfigUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateWorldConfig;

            protected override void HandleMessage(WorldConfig message)
            {
                _ = _server.UpdateWorldConfig(message);
            }

            protected override WorldConfig Parse(ByteString content) => WorldConfig.Parser.ParseFrom(content);
        }

        internal class BroadcastMessageHandler : InternalSessionMasterHandler<DropMessageRequest>
        {
            public BroadcastMessageHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropMessage;

            protected override void HandleMessage(DropMessageRequest message)
            {
                _ = _server.DropWorldMessage(message.Type, message.Message, message.OnlyGM);
            }

            protected override DropMessageRequest Parse(ByteString content) => DropMessageRequest.Parser.ParseFrom(content);
        }

        internal class BroadcastPacketHandler : InternalSessionMasterHandler<PacketRequest>
        {
            public BroadcastPacketHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.BroadcastPacket;

            protected override void HandleMessage(PacketRequest message)
            {
                _ = _server.BroadcastPacket(message);
            }

            protected override PacketRequest Parse(ByteString content) => PacketRequest.Parser.ParseFrom(content);
        }
    }
}

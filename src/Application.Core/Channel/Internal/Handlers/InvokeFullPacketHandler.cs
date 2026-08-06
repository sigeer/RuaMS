using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeFullPacketHandler : InternalSessionChannelHandler<ProtoModel.PacketBroadcastProto>
    {
        public InvokeFullPacketHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.HandleFullPacket;

        protected override Task HandleMessage(ProtoModel.PacketBroadcastProto data)
        {
            var packet = new ByteBufOutPacket(data.Data.ToByteArray());
            return _server.PushChannelCommandAsync(new InvokeChannelBroadcastCommand(data.Receivers, packet));
        }

        protected override ProtoModel.PacketBroadcastProto Parse(ByteString data) => ProtoModel.PacketBroadcastProto.Parser.ParseFrom(data);
    }
}

using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeFullPacketHandler : InternalSessionChannelHandler<PacketBroadcast>
    {
        public InvokeFullPacketHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.HandleFullPacket;

        protected override Task HandleMessage(PacketBroadcast data)
        {
            var packet = new ByteBufOutPacket(data.Data.ToByteArray());
            return _server.PushChannelCommandAsync(new InvokeChannelBroadcastCommand(data.Receivers, packet));
        }

        protected override PacketBroadcast Parse(ByteString data) => PacketBroadcast.Parser.ParseFrom(data);
    }
}

using Application.Shared.Internal;
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

        public override int MessageId => ChannelRecvCode.HandleFullPacket;

        protected override Task HandleAsync(PacketBroadcast data, CancellationToken cancellationToken = default)
        {
            var packet = new ByteBufOutPacket(data.Data.ToByteArray());

            if (data.Receivers.Contains(-1))
            {
                foreach (var player in _server.PlayerStorage.getAllCharacters())
                {
                    player.sendPacket(packet);
                }
            }
            else
            {
                foreach (var id in data.Receivers)
                {
                    _server.PlayerStorage.getCharacterById(id)?.sendPacket(packet);
                }
            }
            return Task.CompletedTask;
        }

        protected override PacketBroadcast Parse(ByteString data) => PacketBroadcast.Parser.ParseFrom(data);
    }
}

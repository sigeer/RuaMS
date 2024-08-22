

using DotNetty.Transport.Channels;

namespace net.encryption;

public class PacketCodec : CombinedChannelDuplexHandler<PacketDecoder, PacketEncoder>
{
    public PacketCodec(ClientCyphers clientCyphers) : base(new PacketDecoder(clientCyphers.getReceiveCypher()), new PacketEncoder(clientCyphers.getSendCypher()))
    {

    }
}

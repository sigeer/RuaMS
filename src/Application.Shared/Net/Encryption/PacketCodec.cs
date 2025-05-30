

using DotNetty.Transport.Channels;

namespace Application.Shared.Net.Encryption;

public class PacketCodec : CombinedChannelDuplexHandler<PacketDecoder, PacketEncoder>
{
    public PacketCodec(ClientCyphers clientCyphers) : base(new PacketDecoder(clientCyphers.getReceiveCypher()), new PacketEncoder(clientCyphers.getSendCypher()))
    {

    }
}

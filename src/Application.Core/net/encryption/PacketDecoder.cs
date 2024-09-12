using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using net.netty;
using net.packet;

namespace net.encryption;

public class PacketDecoder : ReplayingDecoder<DecodingState>
{
    private MapleAESOFB receiveCypher;

    public PacketDecoder(MapleAESOFB receiveCypher) : base(DecodingState.DecodingHeader)
    {
        this.receiveCypher = receiveCypher;
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer inValue, List<object> outs)
    {
        int header = inValue.ReadInt();

        if (!receiveCypher.CheckPacketHeader(header))
        {
            throw new InvalidPacketHeaderException("Attempted to decode a packet with an invalid header", header);
        }

        int packetLength = MapleAESOFB.GetLengthFromPacketHeader(header);
        if (packetLength > inValue.ReadableBytes)
        {
            RequestReplay();
            return;
        }

        byte[] packet = new byte[packetLength];
        inValue.ReadBytes(packet);
        receiveCypher.crypt(packet);
        MapleCustomEncryption.decryptData(packet);
        outs.Add(new ByteBufInPacket(Unpooled.WrappedBuffer(packet)));
    }
}

public enum DecodingState
{
    DecodingHeader,
    DecodingPayload
}
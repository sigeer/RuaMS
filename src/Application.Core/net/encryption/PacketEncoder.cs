using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace net.encryption;

public class PacketEncoder : MessageToByteEncoder<Packet>
{
    private MapleAESOFB sendCypher;

    public PacketEncoder(MapleAESOFB sendCypher)
    {
        this.sendCypher = sendCypher;
    }

    protected override void Encode(IChannelHandlerContext ctx, Packet inValue, IByteBuffer outs)
    {
        byte[] packet = inValue.getBytes();
        outs.WriteBytes(sendCypher.GeneratePacketHeaderFromLength(packet.Length));

        MapleCustomEncryption.encryptData(packet);
        sendCypher.crypt(packet);
        outs.WriteBytes(packet);
    }
}

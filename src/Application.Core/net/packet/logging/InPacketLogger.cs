

using constants.net;
using DotNetty.Transport.Channels;
using tools;

namespace net.packet.logging;

public class InPacketLogger : ChannelHandlerAdapter, PacketLogger
{
    private static ILogger _log = LogFactory.GetLogger("InPacketLogger");
    private static int LOG_CONTENT_THRESHOLD = 3_000;

    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        if (msg is InPacket packet)
        {
            log(packet);
        }

        ctx.FireChannelRead(msg);
    }

    public void log(Packet packet)
    {
        byte[] content = packet.getBytes();
        int packetLength = content.Length;

        if (packetLength <= LOG_CONTENT_THRESHOLD)
        {
            short opcode = LoggingUtil.readFirstShort(content);
            string opcodeHex = opcode.ToString("X2");
            string opcodeName = getRecvOpcodeName(opcode);
            string prefix = opcodeName == null ? "<UnknownPacket> " : "";
            _log.Debug("{Packet}ClientSend:{PacketName} [{PacketCode}] ({PacketLength}) <HEX> {PacketContentHex} <TEXT> {PacketContentString}", prefix, opcodeName, opcodeHex, packetLength,
                    HexTool.toHexString(content), HexTool.toStringFromAscii(content));
        }
        else
        {
            _log.Debug(HexTool.toHexString(new byte[] { content[0], content[1] }) + "...");
        }
    }

    private string getRecvOpcodeName(short opcode)
    {
        return OpcodeConstants.recvOpcodeNames.GetValueOrDefault(opcode);
    }
}

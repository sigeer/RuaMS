using constants.net;
using DotNetty.Transport.Channels;
using tools;

namespace net.packet.logging;

public class OutPacketLogger : ChannelHandlerAdapter, PacketLogger
{
    private static ILogger _log = LogFactory.GetLogger("OutPacketLogger");
    private static int LOG_CONTENT_THRESHOLD = 50_000;

    public override async Task WriteAsync(IChannelHandlerContext context, object message)
    {
        if (message is OutPacket packet)
        {
            log(packet);
        }

        await context.WriteAsync(message);
    }

    public void log(Packet packet)
    {
        byte[] content = packet.getBytes();
        int packetLength = content.Length;

        if (packetLength <= LOG_CONTENT_THRESHOLD)
        {
            short opcode = LoggingUtil.readFirstShort(content);
            string opcodeHex = opcode.ToString("X2");
            string? opcodeName = getSendOpcodeName(opcode);
            string prefix = opcodeName == null ? "<UnknownPacket> " : "";
            _log.Debug("{Prefix}ServerSend:{CodeName} [{Code}] ({Length}) <HEX> {HexContent} <TEXT> {Content}", prefix, opcodeName, opcodeHex, packetLength,
                    HexTool.toHexString(content), HexTool.toStringFromAscii(content));
        }
        else
        {
            _log.Debug(HexTool.toHexString(new byte[] { content[0], content[1] }) + " ...");
        }
    }

    private string? getSendOpcodeName(short opcode)
    {
        return OpcodeConstants.sendOpcodeNames.GetValueOrDefault(opcode);
    }
}

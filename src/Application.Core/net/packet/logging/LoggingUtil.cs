using DotNetty.Buffers;
using net.opcodes;

namespace net.packet.logging;



public class LoggingUtil
{
    private static HashSet<short> ignoredDebugRecvPackets = [
            (short)RecvOpcode.MOVE_PLAYER.getValue(), // 41
        (short)RecvOpcode.HEAL_OVER_TIME.getValue(), // 89
        (short)RecvOpcode.SPECIAL_MOVE.getValue(), // 91
        (short)RecvOpcode.QUEST_ACTION.getValue(), // 107
        (short)RecvOpcode.MOVE_PET.getValue(), // 167
        (short)RecvOpcode.MOVE_LIFE.getValue(), // 188
        (short)RecvOpcode.NPC_ACTION.getValue() // 197
    ];

    public static short readFirstShort(byte[] bytes)
    {
        return Unpooled.WrappedBuffer(bytes).ReadShortLE();
    }

    public static bool isIgnoredRecvPacket(short opcode)
    {
        return ignoredDebugRecvPackets.Contains(opcode);
    }
}

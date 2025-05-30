using System.Drawing;

namespace Application.Shared.Net;

public interface OutPacket : Packet
{
    void writeByte(byte value);
    void writeByte(int value);
    void writeSByte(sbyte value);
    void writeBytes(byte[] value);
    void writeSBytes(sbyte[] value);
    void writeShort(int value);
    void writeInt(int value);
    void writeLong(long value);
    void writeBool(bool value);
    void writeString(string value);
    void writeFixedString(string value, int fix = 13);
    void writePos(Point value);
    void skip(int numberOfBytes);

    static OutPacket create(SendOpcode opcode)
    {
        return new ByteBufOutPacket(opcode);
    }
}

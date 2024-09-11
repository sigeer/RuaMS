

using DotNetty.Buffers;
using net.opcodes;
using System.Text;

namespace net.packet;

public class ByteBufOutPacket : OutPacket
{
    private IByteBuffer byteBuf;

    public ByteBufOutPacket()
    {
        this.byteBuf = Unpooled.Buffer();
    }

    public ByteBufOutPacket(SendOpcode op)
    {
        IByteBuffer byteBuf = Unpooled.Buffer();
        byteBuf.WriteShortLE((short)op.getValue());
        this.byteBuf = byteBuf;
    }

    public ByteBufOutPacket(SendOpcode op, int initialCapacity)
    {
        IByteBuffer byteBuf = Unpooled.Buffer(initialCapacity);
        byteBuf.WriteShortLE((short)op.getValue());
        this.byteBuf = byteBuf;
    }

    public byte[] getBytes()
    {
        byteBuf.MarkReaderIndex();
        var bytes = new byte[byteBuf.ReadableBytes];
        byteBuf.ReadBytes(bytes);
        byteBuf.ResetReaderIndex();
        return bytes;
    }

    public void writeByte(byte value)
    {
        byteBuf.WriteByte(value);
    }

    public void writeByte(int value)
    {
        writeByte((byte)value);
    }

    public void writeSByte(sbyte value)
    {
        writeByte((byte)value);
    }

    public void writeBytes(byte[] value)
    {
        byteBuf.WriteBytes(value);
    }

    public void writeSBytes(sbyte[] value)
    {
        byteBuf.WriteBytes(value.OfType<byte>().ToArray());
    }

    public void writeShort(int value)
    {
        byteBuf.WriteShortLE(value);
    }

    public void writeInt(int value)
    {
        byteBuf.WriteIntLE(value);
    }

    public void writeLong(long value)
    {
        byteBuf.WriteLongLE(value);
    }

    public void writeBool(bool value)
    {
        byteBuf.WriteByte(value ? 1 : 0);
    }

    public void writeString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writeShort(bytes.Length);
        writeBytes(bytes);
    }

    public void writeFixedString(string value)
    {
        writeBytes(Encoding.UTF8.GetBytes(value));
    }

    public void writePos(Point value)
    {
        writeShort((short)value.X);
        writeShort((short)value.Y);
    }

    public void skip(int numberOfBytes)
    {
        writeBytes(new byte[numberOfBytes]);
    }

    public override bool Equals(object? o)
    {
        return o is ByteBufOutPacket other && byteBuf.Equals(other.byteBuf);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return string.Join(", ", getBytes());
    }
}

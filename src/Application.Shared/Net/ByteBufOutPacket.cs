using Application.Utility;
using DotNetty.Buffers;
using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Application.Shared.Net;

public class ByteBufOutPacket : PacketBase, OutPacket
{
    public ByteBufOutPacket() : base(Unpooled.Buffer())
    {
    }

    public ByteBufOutPacket(byte[] bytes) : base(Unpooled.WrappedBuffer(bytes))
    {
    }

    public ByteBufOutPacket(SendOpcode op) : this()
    {
        byteBuf.WriteShortLE((short)op.getValue());
    }

    public ByteBufOutPacket(SendOpcode op, int initialCapacity) : base(Unpooled.Buffer(initialCapacity))
    {
        byteBuf.WriteShortLE((short)op.getValue());
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
        byteBuf.WriteBytes(MemoryMarshal.Cast<sbyte, byte>(value).ToArray());
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
        WriteString(value);
    }

    [Obsolete]
    public void writeStringOld(string value)
    {
        byte[] bytes = GlobalVariable.Encoding.GetBytes(value);
        writeShort(bytes.Length);
        writeBytes(bytes);
    }

    public void WriteString(ReadOnlySpan<char> chars)
    {
        int virtualByteCount = GlobalVariable.Encoding.GetMaxByteCount(chars.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(virtualByteCount);
        try
        {
            int written = GlobalVariable.Encoding.GetBytes(chars, buffer);
            byteBuf.WriteShortLE(written);
            byteBuf.WriteBytes(buffer, 0, written);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void writeFixedString(string value, int fix = 13)
    {
        WriteFixedString(value, fix);
    }

    [Obsolete]
    public void writeFixedStringOld(string value, int fix = 13)
    {
        var bytes = GlobalVariable.Encoding.GetBytes(value ?? string.Empty);
        var fixedBytes = new byte[fix];
        var actualLength = fix > bytes.Length ? bytes.Length : fix;
        Buffer.BlockCopy(bytes, 0, fixedBytes, 0, actualLength);
        writeBytes(fixedBytes);
    }

    public void WriteFixedString(ReadOnlySpan<char> chars, int fix = 13)
    {
        int virtualByteCount = GlobalVariable.Encoding.GetMaxByteCount(chars.Length);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(virtualByteCount);
        try
        {
            int written = GlobalVariable.Encoding.GetBytes(chars, buffer);
            byteBuf.WriteBytes(buffer, 0, Math.Min(written, fix));

            if (fix > written)
                byteBuf.WriteZero(fix - written);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void writePos(Point value)
    {
        writeShort(value.X);
        writeShort(value.Y);
    }

    public void skip(int numberOfBytes)
    {
        byteBuf.WriteZero(numberOfBytes);
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

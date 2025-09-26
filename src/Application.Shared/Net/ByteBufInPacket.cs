using Application.Utility;
using DotNetty.Buffers;
using System.Buffers;
using System.Drawing;
using System.Text;

namespace Application.Shared.Net;

public class ByteBufInPacket : PacketBase, InPacket
{
    public ByteBufInPacket() : base(PooledByteBufferAllocator.Default.Buffer())
    {
    }


    public ByteBufInPacket(byte[] bytes) : this()
    {
        byteBuf.WriteBytes(bytes);
    }

    public byte readByte()
    {
        return byteBuf.ReadByte();
    }

    public sbyte ReadSByte()
    {
        return (sbyte)byteBuf.ReadByte();
    }

    public short readShort()
    {
        return byteBuf.ReadShortLE();
    }

    public int readInt()
    {
        return byteBuf.ReadIntLE();
    }

    public long readLong()
    {
        return byteBuf.ReadLongLE();
    }

    public Point readPos()
    {
        short x = byteBuf.ReadShortLE();
        short y = byteBuf.ReadShortLE();
        return new Point(x, y);
    }

    [Obsolete]
    public string readStringOld()
    {
        short length = readShort();
        var stringBytes = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            byteBuf.ReadBytes(stringBytes, 0, length);
            return GlobalVariable.Encoding.GetString(stringBytes, 0, length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(stringBytes);
        }
    }

    public string readString()
    {
        // 相对readStringOld，少了一次byte[]分配（会直接使用byteBuf.Array）
        short length = readShort();
        return byteBuf.ReadString(length, GlobalVariable.Encoding);
    }

    public byte[] readBytes(int numberOfBytes)
    {
        byte[] bytes = new byte[numberOfBytes];
        byteBuf.ReadBytes(bytes);
        return bytes;
    }

    public void skip(int numberOfBytes)
    {
        byteBuf.SkipBytes(numberOfBytes);
    }

    public int available()
    {
        return byteBuf.ReadableBytes;
    }

    public void seek(int byteOffset)
    {
        byteBuf.SetReaderIndex(byteOffset);
    }

    public int getPosition()
    {
        return byteBuf.ReaderIndex;
    }

    public override bool Equals(object? o)
    {
        return o is ByteBufInPacket other && byteBuf.Equals(other.byteBuf);
    }

    public override string ToString()
    {
        int readerIndex = byteBuf.ReaderIndex;
        byteBuf.MarkReaderIndex();
        byteBuf.SetReaderIndex(0);

        string hexDumpWithPosition = insertReaderPosition(ByteBufferUtil.HexDump(byteBuf).ToUpper(), readerIndex);
        string toString = string.Format("ByteBufInPacket[{0}]", hexDumpWithPosition);

        byteBuf.ResetReaderIndex();
        return toString;
    }

    private static string insertReaderPosition(string hexDump, int index)
    {
        StringBuilder sb = new StringBuilder(hexDump);
        sb.Insert(2 * index, '_');
        return sb.ToString();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

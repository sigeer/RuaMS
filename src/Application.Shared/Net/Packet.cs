using DotNetty.Buffers;

namespace Application.Shared.Net;

public interface Packet
{
    byte[] getBytes();

    IByteBuffer GetByteBuffer();
}

public class PacketBase : Packet
{
    protected IByteBuffer byteBuf;

    public PacketBase(IByteBuffer byteBuf)
    {
        this.byteBuf = byteBuf;
    }

    [Obsolete]
    public byte[] getBytesOld()
    {
        byteBuf.MarkReaderIndex();
        var bytes = new byte[byteBuf.ReadableBytes];
        byteBuf.ReadBytes(bytes);
        byteBuf.ResetReaderIndex();
        return bytes;
    }

    public byte[] getBytes()
    {
        var bytes = new byte[byteBuf.ReadableBytes];
        byteBuf.GetBytes(byteBuf.ReaderIndex, bytes);
        return bytes;
    }

    public IByteBuffer GetByteBuffer()
    {
        return byteBuf;
    }
}
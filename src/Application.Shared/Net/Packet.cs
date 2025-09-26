using DotNetty.Buffers;

namespace Application.Shared.Net;

public interface Packet: IDisposable
{
    byte[] getBytes();
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

    public void Dispose()
    {
        byteBuf.Release();
    }
}
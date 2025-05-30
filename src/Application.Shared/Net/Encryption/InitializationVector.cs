using Application.Utility;

namespace Application.Shared.Net.Encryption;

public class InitializationVector
{
    private byte[] bytes;

    private InitializationVector(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public byte[] getBytes()
    {
        return bytes;
    }

    public static InitializationVector generateSend()
    {
        byte[] ivSend = { 82, 48, 120, getRandomByte() };
        return new InitializationVector(ivSend);
    }

    public static InitializationVector generateReceive()
    {
        byte[] ivRecv = { 70, 114, 122, getRandomByte() };
        return new InitializationVector(ivRecv);
    }

    public static InitializationVector FromBytes(byte[] bytes)
    {
        return new InitializationVector(bytes);
    }

    private static byte getRandomByte()
    {
        #region DEBUG
        return 1;
        #endregion
        return (byte)(Randomizer.nextDouble() * 255);
    }
}

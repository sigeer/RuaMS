namespace tools;

/**
 *
 * @author Shavit
 */
public class LongTool
{

    // Converts 8 bytes to a long.
    public static long BytesToLong(byte[] aToConvert)
    {
        if (aToConvert.Length != 8)
        {
            throw new ArgumentException($"Size of input should be 8, but was {aToConvert.Length}");
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(aToConvert); // 如果系统是小端序，反转字节数组
        }

        return BitConverter.ToInt64(aToConvert, 0);

        //if (aToConvert.Length != 8)
        //{
        //    throw new ArgumentException(string.Format("Size of input should be %d", 1));
        //}

        //long nResult = 0;

        //for (int i = 0; i < 8; i++)
        //{
        //    nResult <<= 8;
        //    nResult |= (aToConvert[i] & 0xFF);
        //}

        //return nResult;
    }

    // Converts a long to 8 bytes.
    public static byte[] LongToBytes(long nToConvert)
    {
        byte[] bytes = BitConverter.GetBytes(nToConvert);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;

        //byte[] aBytes = new byte[8];

        //for(int i = aBytes.Length - 1; i >= 0; i--)
        //{
        //    aBytes[i] = (byte) (nToConvert & 0xFF);
        //    nToConvert >>= 8;
        //}

        //return aBytes;
    }
}

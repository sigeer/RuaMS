namespace Application.Utility.Extensions
{
    public static class ObjectExtensions
    {
        public static sbyte[] ToSBytes(this byte[] byteArray)
        {
            var dp = new sbyte[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                dp[i] = (sbyte)byteArray[i];
            }
            return dp;
        }

        public static byte[] ToBytes(this sbyte[] byteArray)
        {
            var dp = new byte[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                dp[i] = (byte)byteArray[i];
            }
            return dp;
        }
    }
}


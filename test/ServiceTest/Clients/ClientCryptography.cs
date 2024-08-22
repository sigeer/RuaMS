namespace ServiceTest.Clients
{
    /// <summary>
    /// https://github.com/FoxyYokai/HeavenClient/blob/main/Net/Cryptography.cpp
    /// </summary>
    public class ClientCryptography
    {
        private const int HEADER_LENGTH = 4;
        private const byte MAPLEVERSION = 83;
        private byte[] sendiv = new byte[HEADER_LENGTH];
        private byte[] recviv = new byte[HEADER_LENGTH];


        public ClientCryptography(byte[] handshake)
        {

            Array.Copy(handshake, 7, sendiv, 0, HEADER_LENGTH);
            Array.Copy(handshake, 11, recviv, 0, HEADER_LENGTH);

        }

        public void CreateHeader(byte[] buffer, int length)
        {
            int a = ((sendiv[3] << 8) | sendiv[2]) ^ MAPLEVERSION;
            int b = a ^ length;

            buffer[0] = (byte)(a % 0x100);
            buffer[1] = (byte)(a / 0x100);
            buffer[2] = (byte)(b % 0x100);
            buffer[3] = (byte)(b / 0x100);
        }

        public int CheckLength(byte[] bytes)
        {
            int headerMask = 0;

            for (int i = 0; i < 4; i++)
            {
                headerMask |= bytes[i] << (8 * i);
            }

            return (headerMask >> 16) ^ (headerMask & 0xFFFF);
        }
    }

}

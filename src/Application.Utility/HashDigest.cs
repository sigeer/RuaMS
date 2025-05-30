using System.Security.Cryptography;
using System.Text;

namespace Application.Utility
{
    public static class HashDigest
    {
        public static byte[] HashByType(string type, string str)
        {
            HashAlgorithm? hashAlgorithm = null;
            try
            {
                switch (type.ToUpper())
                {
                    case "SHA-512":
                        hashAlgorithm = SHA512.Create();
                        break;
                    case "SHA-256":
                        hashAlgorithm = SHA256.Create();
                        break;
                    case "SHA-1":
                        hashAlgorithm = SHA1.Create();
                        break;
                    case "MD5":
                        hashAlgorithm = MD5.Create();
                        break;
                    default:
                        throw new ArgumentException("Unsupported hash type");
                }
                return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
            finally
            {
                hashAlgorithm?.Dispose();
            }
        }
    }
}

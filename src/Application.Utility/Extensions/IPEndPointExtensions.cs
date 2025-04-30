using System.Net;
using System.Net.Sockets;

namespace Application.Utility.Extensions
{
    public static class IPEndPointExtensions
    {
        public static string GetIPString(this IPEndPoint iPEndPoint)
        {
            if (iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6 && iPEndPoint.Address.IsIPv4MappedToIPv6)
                return iPEndPoint.Address.MapToIPv4().ToString();

            return iPEndPoint.Address.ToString();
        }

        public static string GetIPAddressString(this EndPoint endPoint)
        {
            if (endPoint is IPEndPoint iPEndPoint)
                return iPEndPoint.GetIPString();

            throw new Exception($"EndPoint: {endPoint} convert to IPEndPoint failure");
        }

        public static bool IsPrivateIP(this IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
            {
                return (bytes[0] == 10) ||
                       (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                       (bytes[0] == 192 && bytes[1] == 168);
            }
            else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) // IPv6
            {
                return bytes[0] == 0xFC || bytes[0] == 0xFD ||
                       (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80);
            }
            return false;
        }
    }
}

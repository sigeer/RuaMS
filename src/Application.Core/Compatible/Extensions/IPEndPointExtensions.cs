using System.Net;
using System.Net.Sockets;

namespace Application.Core.Compatible.Extensions
{
    public static class IPEndPointExtensions
    {
        public static string GetIPv4(this IPEndPoint iPEndPoint)
        {
            if (iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                return iPEndPoint.Address.MapToIPv4().ToString();

            return iPEndPoint.Address.ToString();
        }

        public static string GetIPv4Address(this EndPoint endPoint)
        {
            if (endPoint is IPEndPoint iPEndPoint)
                return iPEndPoint.GetIPv4();

            throw new Exception($"EndPoint: {endPoint} convert to IPEndPoint failure");
        }
    }
}

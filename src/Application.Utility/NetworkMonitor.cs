using Application.Utility.Extensions;
using System.Net;
using System.Net.NetworkInformation;

namespace Application.Utility
{
    public class NetworkMonitor
    {
        private class InterfaceStats
        {
            public long PreviousSent;
            public long PreviousReceived;
        }

        private readonly Dictionary<string, InterfaceStats> _prevStats = new();
        private readonly List<NetworkInterface> _interfaces = new();

        public NetworkMonitor()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 只关注已启用且非环回接口
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    _interfaces.Add(ni);
                    var stats = ni.GetIPv4Statistics();
                    _prevStats[ni.Id] = new InterfaceStats
                    {
                        PreviousSent = stats.BytesSent,
                        PreviousReceived = stats.BytesReceived
                    };
                }
            }
        }

        /// <summary>
        /// 获取内网和外网流量速率（单位：Bytes/s）
        /// </summary>
        public async Task<(long internalSent, long internalRecv, long externalSent, long externalRecv)> GetTrafficRateAsync(int intervalMs = 1000)
        {
            var internalSent = 0L;
            var internalRecv = 0L;
            var externalSent = 0L;
            var externalRecv = 0L;

            // 等待采样间隔
            await Task.Delay(intervalMs);

            foreach (var ni in _interfaces)
            {
                var stats = ni.GetIPv4Statistics();
                var prev = _prevStats[ni.Id];

                long sentDiff = stats.BytesSent - prev.PreviousSent;
                long recvDiff = stats.BytesReceived - prev.PreviousReceived;

                bool isInternal = false;
                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        if (addr.Address.IsPrivateIP())
                        {
                            isInternal = true;
                            break;
                        }
                    }
                }

                if (isInternal)
                {
                    internalSent += sentDiff;
                    internalRecv += recvDiff;
                }
                else
                {
                    externalSent += sentDiff;
                    externalRecv += recvDiff;
                }

                // 更新上次统计
                prev.PreviousSent = stats.BytesSent;
                prev.PreviousReceived = stats.BytesReceived;
            }

            // 计算每秒速率
            double factor = 1000.0 / intervalMs;
            return ((long)(internalSent * factor),
                    (long)(internalRecv * factor),
                    (long)(externalSent * factor),
                    (long)(externalRecv * factor));
        }
    }
}

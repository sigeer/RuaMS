using Application.Shared.Net;

namespace Application.Shared.Servers
{
    public interface IServerTransport
    {

        /// <summary>
        /// 获取服务器当前时间（以调度服务器为主，相当于把之前的Server类改为调度服务器）
        /// </summary>
        /// <returns></returns>
        long GetServerCurrentTime();
        int GetServerCurrentTimestamp();
        DateTimeOffset GetServerUpTime();
        /// <summary>
        /// 向全服发送数据包
        /// </summary>
        /// <param name="p"></param>
        void BroadcastMessage(Packet p);
        /// <summary>
        /// 全服GM发送数据包
        /// </summary>
        /// <param name="p"></param>
        void BroadcastGMMessage(Packet p);

        void DropMessage(int type, string message);

        void BroadcastMessage(IEnumerable<int> playerIdList, Packet p);
    }
}

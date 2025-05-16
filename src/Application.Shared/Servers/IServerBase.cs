using Application.Shared.Net;
using System.Net.Sockets;

namespace Application.Shared.Servers
{
    public interface IServerBase<out TServerTransport> where TServerTransport : IServerTransport
    {

        string InstanceId { get; }
        TServerTransport Transport { get; }
        Task StartServer();
        Task Shutdown();
        DateTimeOffset StartupTime { get; }
        AbstractServer NettyServer { get; }
        bool IsRunning { get; }
        int Port { get; set; }
        /// <summary>
        /// 已运行时长
        /// </summary>
        /// <returns></returns>
        int getCurrentTimestamp();
        long getCurrentTime();
        void SetCharacteridInTransition(string v, int cid);
        bool HasCharacteridInTransition(string clientSession);
        void UpdateAccountState(int accId, sbyte state);

        void BroadcastWorldGMPacket(Packet packet);
        bool CheckCharacterName(string name);
        void UpdateAccountChracterByAdd(int accountId, int id);
    }
}

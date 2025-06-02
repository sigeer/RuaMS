using Application.Shared.Login;
using Application.Shared.Models;
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
        /// <summary>
        /// 当前服务器的启动时间(StartServer之后)
        /// </summary>
        DateTimeOffset StartupTime { get; }
        AbstractServer NettyServer { get; }
        bool IsRunning { get; }
        int Port { get; set; }
        /// <summary>
        /// 已运行时长（已主服务器为准）
        /// </summary>
        /// <returns></returns>
        int getCurrentTimestamp();
        long getCurrentTime();
        void UpdateServerTime();
        /// <summary>
        /// 主服务器强制更新、频道服务器强制从主服务器获取
        /// </summary>
        void ForceUpdateServerTime();
        void SetCharacteridInTransition(string v, int cid);
        bool HasCharacteridInTransition(string clientSession);
        AccountLoginStatus UpdateAccountState(int accId, sbyte state);

        void BroadcastWorldGMPacket(Packet packet);
        void BroadcastWorldMessage(Packet p);
        bool CheckCharacterName(string name);
        void UpdateAccountChracterByAdd(int accountId, int id);
        void CommitAccountEntity(AccountCtrl accountEntity);
    }
}

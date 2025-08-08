using Application.Shared.Login;
using Application.Shared.Models;
using Application.Shared.Net;

namespace Application.Shared.Servers
{
    public interface IServerBase<out TServerTransport> where TServerTransport : IServerTransport
    {
        string ServerName { get; }
        TServerTransport Transport { get; }
        Task StartServer();
        Task Shutdown(int delaySeconds = -1);
        bool IsRunning { get; }

        /// <summary>
        /// 当前服务器的启动时间(StartServer之后)
        /// </summary>
        DateTimeOffset StartupTime { get; }
        /// <summary>
        /// 已运行时长（以主服务器为准）
        /// </summary>
        /// <returns></returns>
        int getCurrentTimestamp();
        long getCurrentTime();
        void UpdateServerTime();
        /// <summary>
        /// 主服务器强制更新、频道服务器强制从主服务器获取
        /// </summary>
        void ForceUpdateServerTime();
        void SetCharacteridInTransition(string clientSession, int cid);
        bool HasCharacteridInTransition(string clientSession);
        AccountLoginStatus UpdateAccountState(int accId, sbyte state);
    }
}

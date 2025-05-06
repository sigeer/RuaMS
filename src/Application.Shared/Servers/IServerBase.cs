using Application.Shared.Net;
using System.Net;

namespace Application.Shared.Servers
{
    /// <summary>
    /// 玩家实际连接的服务器，登录服务器（调度服务器）、频道服务器
    /// </summary>
    public interface IServerBase<out TTransport> where TTransport : IServerTransport
    {
        string InstanceId { get; }
        TTransport Transport { get; }
        int Port { get; }
        int World { get; }
        int Channel { get; }
        public DateTimeOffset StartTime { get; }
        AbstractServer NettyServer { get; }
        Task StartServer();
        Task Shutdown();
    }
}

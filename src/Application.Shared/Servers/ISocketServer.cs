namespace Application.Shared.Servers
{
    public interface ISocketServer
    {
        int Port { get; set; }
        AbstractNettyServer NettyServer { get; }
        Task StartServer();
        Task ShutdownServer();
        bool IsRunning { get; }
    }
}

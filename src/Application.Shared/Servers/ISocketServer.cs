namespace Application.Shared.Servers
{
    public interface ISocketServer
    {
        int Port { get; set; }
        AbstractNettyServer NettyServer { get; }
        Task StartServer(CancellationToken cancellationToken);
        Task ShutdownServer();
        bool IsRunning { get; }
    }
}

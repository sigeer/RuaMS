namespace Application.Shared.Servers
{
    public interface ISocketServer
    {
        int Port { get; set; }
        AbstractNettyServer NettyServer { get; }
        Task StartServer(CancellationToken cancellationToken);
        Task Shutdown(int delaySeconds = -1);
        bool IsRunning { get; }
    }
}

namespace Application.Shared.Servers
{
    public interface ISocketServer
    {
        int Port { get; set; }
        AbstractNettyServer NettyServer { get; }
        Task StartServer();
        Task Shutdown();
        bool IsRunning { get; }
    }
}

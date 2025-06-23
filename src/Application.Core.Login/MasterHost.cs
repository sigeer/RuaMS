using Microsoft.Extensions.Hosting;

namespace Application.Core.Login
{
    internal class MasterHost : IHostedService
    {
        readonly MasterServer _server;
        public MasterHost(MasterServer server)
        {
            _server = server;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _server.StartServer();
            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _server.Shutdown();
        }
    }
}

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
            await StartNow(true);
            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopNow();
        }
        public async Task StartNow(bool ignoreCache)
        {
            await _server.StartServer();
        }

        public async Task StopNow()
        {
            await _server.Shutdown();
        }
    }
}

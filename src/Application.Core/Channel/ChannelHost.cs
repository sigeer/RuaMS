using Microsoft.Extensions.Hosting;

namespace Application.Core.Channel
{
    internal class ChannelHost : IHostedService
    {
        readonly WorldChannelServer _server;
        public ChannelHost(WorldChannelServer server)
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

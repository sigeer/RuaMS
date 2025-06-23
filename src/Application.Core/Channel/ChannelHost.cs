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
            await _server.StartServer();
            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _server.Shutdown();
        }
    }
}

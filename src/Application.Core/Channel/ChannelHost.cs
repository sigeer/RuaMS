using Microsoft.Extensions.Hosting;

namespace Application.Core.Channel
{
    public class ChannelHost : IHostedService
    {
        readonly WorldChannelServer _server;
        readonly IHostApplicationLifetime _hostLifetime;
        public ChannelHost(WorldChannelServer server, IHostApplicationLifetime hostLifetime)
        {
            _server = server;
            _hostLifetime = hostLifetime;

            _hostLifetime.ApplicationStopping.Register(() =>
            {
                _server.Shutdown().GetAwaiter().GetResult();
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _server.StartServer(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

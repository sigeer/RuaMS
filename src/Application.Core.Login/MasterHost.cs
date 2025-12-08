using Microsoft.Extensions.Hosting;

namespace Application.Core.Login
{
    public class MasterHost : IHostedService
    {
        readonly MasterServer _server;
        readonly IHostApplicationLifetime _hostLifetime;
        public MasterHost(MasterServer server, IHostApplicationLifetime hostApplicationLifetime)
        {
            _server = server;
            _hostLifetime = hostApplicationLifetime;

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

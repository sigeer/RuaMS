using Application.Core.Channel;
using Application.Core.Login;

namespace Application.Host
{
    public class GameHost : IHostedService
    {
        readonly MasterServer _server;
        readonly WorldChannelServer _channelRunner;
        readonly IHostApplicationLifetime _hostApplicationLifetime;

        public GameHost(MasterServer server, WorldChannelServer channelRunner, IHostApplicationLifetime hostApplicationLifetime)
        {
            _server = server;
            _channelRunner = channelRunner;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                _server.Shutdown().Wait();
            });

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
            await _channelRunner.Start(7674, 3);
        }

        public async Task StopNow()
        {
            await _server.Shutdown();
        }
    }
}

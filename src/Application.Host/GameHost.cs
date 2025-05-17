using Application.Core.Channel;
using Application.Core.Servers;
using net.server;

namespace Application.Host
{
    public class GameHost : IHostedService
    {
        readonly IMasterServer _server;
        readonly MultiRunner _channelRunner;

        public GameHost(IMasterServer server, MultiRunner channelRunner)
        {
            _server = server;
            _channelRunner = channelRunner;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Server.getInstance().Start();
            await _server.StartServer();
            await _channelRunner.Start(7674, 3);
            return;
        }

        public async Task StartNow(bool ignoreCache)
        {
            await Server.getInstance().Start(ignoreCache);
        }

        public async Task StopNow()
        {
            await _server.Shutdown();
            await Server.getInstance().Stop(false);
        }
        public async Task Restart()
        {
            await Server.getInstance().Stop(true);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _server.Shutdown();
            await Server.getInstance().Stop(false);
        }
    }
}

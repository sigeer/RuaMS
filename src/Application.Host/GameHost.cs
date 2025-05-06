
using net.server;

namespace Application.Host
{
    public class GameHost : IHostedService
    {
        readonly ServerCenter _center;

        public GameHost(ServerCenter center)
        {
            _center = center;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _center.StartServer();
        }

        public async Task StartNow(bool ignoreCache)
        {
            await Server.getInstance().Start(ignoreCache);
        }

        public async Task StopNow()
        {
            await Server.getInstance().Stop(false);
        }
        public async Task Restart()
        {
            await Server.getInstance().Stop(true);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Server.getInstance().Stop(false);
        }
    }
}

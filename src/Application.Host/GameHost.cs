
using net.server;

namespace Application.Host
{
    public class GameHost : IHostedService
    {

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await Server.getInstance().Start();
            });
            return Task.CompletedTask ;
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

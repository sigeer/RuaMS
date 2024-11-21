
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

        public async Task StartNow()
        {
            await Server.getInstance().Start();
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

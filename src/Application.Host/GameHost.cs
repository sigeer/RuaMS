
using net.server;

namespace Application.Host
{
    public class GameHost : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Server.getInstance().Start();

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Server.getInstance().Stop(false);
        }
    }
}

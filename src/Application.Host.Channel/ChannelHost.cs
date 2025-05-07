
using Application.Core.Game.TheWorld;

namespace Application.Host.Channel
{
    public class ChannelHost : IHostedService
    {
        readonly IWorldChannel _channelServer;

        public ChannelHost(IWorldChannel channelServer)
        {
            _channelServer = channelServer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _channelServer.StartServer();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _channelServer.Shutdown();
        }
    }
}

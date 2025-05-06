
using Application.Core.Game.TheWorld;
using Application.Shared.Servers;
using net.server.channel;

namespace Application.Server.Channel
{
    public class ChannelHost : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var channel = new WorldChannel(new ActualServerConfig
            {
                Port = 7474,
            }, new RemoteChannelServerTransport(""));
            await channel.StartServer();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

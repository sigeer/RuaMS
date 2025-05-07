using Application.Core.Channel;
using Application.Core.Channel.ServerTransports;
using Application.Core.Servers;
using net.server;

namespace Application.Host
{
    public class GameHost : IHostedService
    {
        readonly IMasterServer _server;

        public GameHost(IMasterServer server)
        {
            _server = server;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Server.getInstance().Start();
            await _server.StartServer();
            var world = Server.getInstance().getWorld(0);
            for (int j = 1; j <= 3; j++)
            {
                int channelid = j;
                var channel = new WorldChannel(new ChannelServerConfig
                {
                    Port = 7574 + channelid
                }, new LocalChannelServerTransport(_server, world));
                await channel.StartServer();
            }
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

using Application.Core.Channel;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel
{
    public class MultiRunner
    {
        readonly IServiceProvider _sp;
        readonly IChannelServerTransport _transport;
        List<WorldChannel> _localChannels;

        public MultiRunner(IServiceProvider sp, IChannelServerTransport transport)
        {
            _sp = sp;
            _transport = transport;

            _localChannels = new List<WorldChannel>();
        }

        public async Task Start(int startPort = 7574, int count = 3)
        {
            for (int j = 1; j <= count; j++)
            {
                var config = new ChannelServerConfig
                {
                    Port = startPort + j
                };
                var scope = _sp.CreateScope();
                var channel = new WorldChannel(scope, config, _transport);
                _localChannels.Add(channel);
                await channel.StartServer();
            }
        }
    }
}

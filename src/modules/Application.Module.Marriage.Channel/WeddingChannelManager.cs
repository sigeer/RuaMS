using Application.Core.Channel;
using Application.Core.Scripting.Events;
using Application.Module.Marriage.Channel.Models;
using Microsoft.Extensions.Logging;
using tools.exceptions;

namespace Application.Module.Marriage.Channel
{
    public class WeddingChannelManager
    {
        readonly ILogger<WeddingChannelManager> _logger;

        readonly WorldChannelServer _server;
        readonly IModuleChannelServerTransport _transport;
        public WorldChannel ChannelServer { get; }
        public List<WeddingInfo> RegisteredWeddings { get; set; } = [];
        public WeddingChannelManager(WorldChannelServer server, ILogger<WeddingChannelManager> logger, WorldChannel worldChannel, IModuleChannelServerTransport transport)
        {
            _server = server;
            _logger = logger;
            ChannelServer = worldChannel;
            _transport = transport;
        }
    }
}

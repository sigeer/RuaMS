using Application.Core.Channel;
using Application.Module.Marriage.Channel.Models;
using Microsoft.Extensions.Logging;
using scripting.Event;
using tools.exceptions;

namespace Application.Module.Marriage.Channel
{
    public class WeddingChannelManager
    {
        readonly ILogger<WeddingChannelManager> _logger;

        readonly WorldChannelServer _server;
        public WorldChannel ChannelServer { get; }
        public List<WeddingInfo> RegisteredWeddings { get; set; } = [];
        public WeddingChannelManager(WorldChannelServer server, ILogger<WeddingChannelManager> logger, WorldChannel worldChannel)
        {
            _server = server;
            _logger = logger;
            ChannelServer = worldChannel;
        }

        public MarriageInstance CreateMarriageInstance(EventManager em, string name)
        {
            MarriageInstance ret = new MarriageInstance(em, name);

            if (em.RegisterInstance(name, ret))
                throw new EventInstanceInProgressException(name, em.getName());
            return ret;
        }
    }
}

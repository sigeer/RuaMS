using Application.Core.Channel;
using Application.Core.Channel.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Module.Maker.Channel
{
    internal class MakerChannelModule : ChannelModule
    {
        public MakerChannelModule(WorldChannelServer server, ILogger<ChannelModule> logger) : base(server, logger)
        {
        }
    }
}

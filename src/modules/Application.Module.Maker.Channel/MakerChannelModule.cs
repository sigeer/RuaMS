using Application.Core.Channel;
using Application.Core.Channel.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Module.Maker.Channel
{
    internal class MakerChannelModule : AbstractChannelModule
    {
        public MakerChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger) : base(server, logger)
        {
        }
    }
}

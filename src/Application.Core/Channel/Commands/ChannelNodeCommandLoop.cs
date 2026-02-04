using Application.Utility.Pipeline;

namespace Application.Core.Channel.Commands
{
    public class ChannelNodeCommandLoop : CommandLoop<ChannelNodeCommandContext>
    {
        WorldChannelServer _server;
        public ChannelNodeCommandLoop(WorldChannelServer server) : base()
        {
            _server = server;
        }

        protected override ChannelNodeCommandContext CreateContext()
        {
            return new ChannelNodeCommandContext(_server);
        }
    }

    public class WorldChannelCommandLoop : CommandLoop<ChannelCommandContext>
    {
        WorldChannel _channel;

        public WorldChannelCommandLoop(WorldChannel channel)
        {
            _channel = channel;
        }

        protected override ChannelCommandContext CreateContext()
        {
            return new ChannelCommandContext(_channel);
        }
    }
}

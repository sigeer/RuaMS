using Application.Utility.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    public class ChannelNodeCommandContext : ICommandContext
    {
        public WorldChannelServer Server { get; }
        public ChannelNodeCommandContext(WorldChannelServer server)
        {
            Server = server;
        }
    }

    public class ChannelCommandContext: ICommandContext
    {
        public ChannelCommandContext(WorldChannel worldChannel)
        {
            WorldChannel = worldChannel;
        }

        public WorldChannel WorldChannel { get; }
    }
}

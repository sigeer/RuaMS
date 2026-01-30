using server.maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class ReactorHitCommand : IWorldChannelCommand
    {
        Reactor _reactor;
        IChannelClient _client;

        public ReactorHitCommand(Reactor reactor, IChannelClient client)
        {
            _reactor = reactor;
            _client = client;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _reactor.hitReactor(_client);
        }
    }
}

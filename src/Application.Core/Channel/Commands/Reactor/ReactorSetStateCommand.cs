using server.maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class ReactorSetStateCommand : IWorldChannelCommand
    {
        Reactor _reactor;
        sbyte _nextState;

        public ReactorSetStateCommand(Reactor reactor, sbyte nextState)
        {
            _reactor = reactor;
            _nextState = nextState;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _reactor.tryForceHitReactor(_nextState);
        }
    }
}

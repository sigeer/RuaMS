using server.maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class ReactorRespawnCommand: IWorldChannelCommand
    {
        Reactor _reactor;
        bool _fromDestroyed;

        public ReactorRespawnCommand(Reactor reactor, bool fromDestroyed)
        {
            _reactor = reactor;
            _fromDestroyed = fromDestroyed;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _reactor.respawn(_fromDestroyed);
        }
    }
}

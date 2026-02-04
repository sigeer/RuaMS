using Application.Core.Game.Maps;
using server.maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class ReactorHitFromMapItemCommand : IWorldChannelCommand
    {
        MapItem _mapItem;
        Reactor _reactor;

        public ReactorHitFromMapItemCommand(MapItem mapItem, Reactor reactor)
        {
            _mapItem = mapItem;
            _reactor = reactor;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _reactor.HitByMapItem(_mapItem);
        }
    }
}

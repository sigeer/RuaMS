using Application.Core.Game.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MapItemExpiredCommand : IWorldChannelCommand
    {
        readonly IMap _map;

        public MapItemExpiredCommand(IMap map)
        {
            _map = map;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.makeDisappearExpiredItemDrops();
        }
    }
}

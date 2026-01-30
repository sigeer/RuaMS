using server.maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MapManagerDisposeCommand : IWorldChannelCommand
    {
        MapManager _mapManager;

        public MapManagerDisposeCommand(MapManager mapManager)
        {
            _mapManager = mapManager;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mapManager.Dispose();
        }
    }
}

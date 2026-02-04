using server.maps;

namespace Application.Core.Channel.Commands
{
    public class RespawnCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            var ps = ctx.WorldChannel.getPlayerStorage();
            if (ps != null)
            {
                if (ps.getAllCharacters().Count() > 0)
                {
                    MapManager mapManager = ctx.WorldChannel.getMapFactory();
                    if (mapManager != null)
                    {
                        mapManager.updateMaps();
                    }
                }
            }
        }
    }
}

using Application.Core.Game.Controllers;
using Application.Core.Game.TheWorld;
using Application.Utility.Configs;
using server.maps;

namespace Application.Core.Channel.Tasks
{
    public class RespawnTask : TimelyControllerBase
    {
        readonly IWorldChannel _worldChannel;

        public RespawnTask(IWorldChannel worldChannel)
            : base($"RespawnTask_{worldChannel.InstanceId}",
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.RESPAWN_INTERVAL),
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.RESPAWN_INTERVAL))
        {
            _worldChannel = worldChannel;
        }

        protected override void HandleRun()
        {
            var ps = _worldChannel.getPlayerStorage();
            if (ps != null)
            {
                if (ps.getAllCharacters().Count() > 0)
                {
                    MapManager mapManager = _worldChannel.getMapFactory();
                    if (mapManager != null)
                    {
                        mapManager.updateMaps();
                    }
                }
            }
        }
    }

}
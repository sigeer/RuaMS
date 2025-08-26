using server.maps;

namespace Application.Core.Channel.Tasks
{
    public class RespawnTask : TaskBase
    {
        readonly WorldChannel _worldChannel;

        public RespawnTask(WorldChannel worldChannel)
            : base($"Channel:{worldChannel.Id}_{nameof(RespawnTask)}",
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
using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class RespawnTask : TaskBase
    {
        readonly WorldChannel _worldChannel;

        public RespawnTask(WorldChannel worldChannel)
            : base($"{worldChannel.InstanceName}_{nameof(RespawnTask)}",
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.RESPAWN_INTERVAL),
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.RESPAWN_INTERVAL))
        {
            _worldChannel = worldChannel;
        }

        protected override void HandleRun()
        {
            _worldChannel.Post(new RespawnCommand());
        }
    }
}
using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class ChannelTickableTask : TaskBase
    {
        readonly WorldChannel _worldChannel;

        public ChannelTickableTask(WorldChannel worldChannel)
            : base(nameof(ChannelTickableTask),
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC),
                  TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC))
        {
            _worldChannel = worldChannel;
        }

        protected override void HandleRun()
        {
            _worldChannel.Send(new OnChannelTickCommand());
        }
    }
}

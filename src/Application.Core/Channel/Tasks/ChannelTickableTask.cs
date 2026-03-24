using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class ChannelTickableTask : TaskBase
    {
        readonly WorldChannel _worldChannel;

        public ChannelTickableTask(WorldChannel worldChannel)
            : base(nameof(ChannelTickableTask),
                  TimeSpan.FromMilliseconds(250),
                  TimeSpan.FromMilliseconds(250))
        {
            _worldChannel = worldChannel;
        }

        protected override void HandleRun()
        {
            _worldChannel.Post(new OnChannelTickCommand());
        }
    }
}
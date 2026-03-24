using Application.Core.Channel.Commands.Channel;

namespace Application.Core.Channel.Tasks
{
    internal class ChannelTickTask : TaskBase
    {
        readonly WorldChannel _channel;
        public ChannelTickTask(WorldChannel worldChannel) : base(nameof(ChannelTickTask),
                  TimeSpan.FromSeconds(1),
                  TimeSpan.FromSeconds(1))
        {
            _channel = worldChannel;
        }

        protected override void HandleRun()
        {
            _channel.Post(new InvokeExpirationCheckCommand());
        }
    }
}

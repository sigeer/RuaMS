using Config;

namespace Application.Core.Channel.Commands
{
    internal class InvokeWorldConfigUpdateCommand : IWorldChannelCommand
    {
        Config.WorldConfig _config;

        public InvokeWorldConfigUpdateCommand(WorldConfig config)
        {
            _config = config;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.UpdateWorldConfig(_config);
        }
    }
}

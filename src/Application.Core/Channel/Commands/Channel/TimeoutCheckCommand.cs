namespace Application.Core.Channel.Commands
{
    internal class TimeoutCheckCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            var chars = ctx.WorldChannel.getPlayerStorage().getAllCharacters();
            foreach (var chr in chars)
            {
                if (ctx.WorldChannel.Node.GetCurrentTimeDateTimeOffset() - chr.getClient().LastPacket > TimeSpan.FromMilliseconds(YamlConfig.config.server.TIMEOUT_DURATION))
                {
                    Log.Logger.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                    chr.getClient().Disconnect(true, chr.getCashShop().isOpened());
                }
            }
        }
    }
}

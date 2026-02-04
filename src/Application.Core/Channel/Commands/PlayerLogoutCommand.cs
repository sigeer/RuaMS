namespace Application.Core.Channel.Commands
{
    internal class PlayerLogoutCommand : IWorldChannelCommand
    {
        int _chrId;

        public PlayerLogoutCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var mc = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (mc != null)
            {
                mc.RemoveWorldWatcher();
                mc.setClient(new OfflineClient());
            }
        }
    }
}

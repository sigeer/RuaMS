namespace Application.Core.Channel.Commands
{
    internal class PlayerMountTirednessCalcCommand : IWorldChannelCommand
    {
        int chrId;

        public PlayerMountTirednessCalcCommand(int chrId)
        {
            this.chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(chrId);
            if (chr != null)
            {
                chr.runTirednessSchedule();
            }
            return;
        }
    }
}

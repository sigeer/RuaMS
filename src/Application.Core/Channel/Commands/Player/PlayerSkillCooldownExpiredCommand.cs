namespace Application.Core.Channel.Commands
{
    internal class PlayerSkillCooldownExpiredCommand : IWorldChannelCommand
    {
        Player _chr;

        public PlayerSkillCooldownExpiredCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ClearExpiredSkillCooldown();
        }
    }
}

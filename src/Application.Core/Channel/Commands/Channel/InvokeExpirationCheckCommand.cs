namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokeExpirationCheckCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            var now = ctx.WorldChannel.Node.getCurrentTime();
            var allPlayers = ctx.WorldChannel.Players.getAllCharacters();
            foreach (var chr in allPlayers)
            {
                chr.ClearExpiredBuffs(now);
                chr.ClearExpiredDisease(now);
                chr.ClearExpiredItems(now);
                chr.ClearExpiredSkills(now);
                chr.ClearExpiredQuests(now);
                chr.ClearExpiredSkillCooldown(now);
            }
        }
    }
}

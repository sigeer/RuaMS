using server.quest;

namespace Application.Core.Channel.Commands
{
    internal class PlayerQuestExpiredCommand : IWorldChannelCommand
    {
        Player _chr;
        public PlayerQuestExpiredCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ClearExpiredQuests();
        }
    }
}

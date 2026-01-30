using server;

namespace Application.Core.Channel.Commands
{
    internal class PlayerBeholdHealBuffCommand : IWorldChannelCommand
    {
        int _chrId;
        StatEffect _buffEffect;

        public PlayerBeholdHealBuffCommand(int chrId, StatEffect buffEffect)
        {
            _chrId = chrId;
            _buffEffect = buffEffect;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId)?.ApplyBeholderHeal(_buffEffect);
        }
    }
}

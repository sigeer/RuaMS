using server;

namespace Application.Core.Channel.Commands
{
    internal class PlayerBeholdHexBuffCommand : IWorldChannelCommand
    {
        int _chrId;
        StatEffect _buffEffect;

        public PlayerBeholdHexBuffCommand(int chrId, StatEffect buffEffect)
        {
            _chrId = chrId;
            _buffEffect = buffEffect;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId)?.ApplyBeholderHex(_buffEffect);
        }
    }
}

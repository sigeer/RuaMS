using server;

namespace Application.Core.Channel.Commands
{
    internal class PlayerBerserkBuffCommand : IWorldChannelCommand
    {
        bool _hidden;
        int _chrId;
        StatEffect _buffEffect;
        bool _isBerserk;

        public PlayerBerserkBuffCommand(bool hidden, int chrId, StatEffect buffEffect, bool isBerserk)
        {
            _hidden = hidden;
            _chrId = chrId;
            _buffEffect = buffEffect;
            _isBerserk = isBerserk;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId)?.ApplyBerserkBuff(_hidden, _buffEffect, _isBerserk);
        }
    }
}

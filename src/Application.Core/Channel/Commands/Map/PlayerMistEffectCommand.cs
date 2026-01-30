using Application.Core.Game.Maps.Mists;

namespace Application.Core.Channel.Commands
{
    internal class PlayerMistEffectCommand : IWorldChannelCommand
    {
        PlayerMist _playerMist;

        public PlayerMistEffectCommand(PlayerMist playerMist)
        {
            _playerMist = playerMist;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _playerMist.ApplyMistEffect();
        }
    }
}

using Application.Core.Game.Life;

namespace Application.Core.Channel.Commands
{
    internal class MonsterReviveCommand : IWorldChannelCommand
    {
        Monster _mob;
        Player? _killer;

        public MonsterReviveCommand(Monster mob, Player? killer)
        {
            _mob = mob;
            _killer = killer;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.Revive(_killer);
        }
    }
}

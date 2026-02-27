using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MonsterReviveCommand : IWorldChannelCommand
    {
        Monster _mob;
        ICombatantObject? _killer;
        MonsterControllerPair _lastController;

        public MonsterReviveCommand(Monster mob, ICombatantObject? killer, MonsterControllerPair lastController)
        {
            _mob = mob;
            _killer = killer;
            _lastController = lastController;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.Revive(_killer, _lastController);
        }
    }
}

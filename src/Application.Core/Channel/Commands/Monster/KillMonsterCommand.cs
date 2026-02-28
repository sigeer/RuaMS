using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class KillMonsterCommand: IWorldChannelCommand
    {
        IMap _map;
        Monster _targetMob;
        Player? _killer;
        int _animate;

        public KillMonsterCommand(IMap map, Monster targetMob, Player? killer, int animate)
        {
            _map = map;
            _targetMob = targetMob;
            _killer = killer;
            _animate = animate;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.RemoveMob(_targetMob, _killer, true, _animate, 0);
        }
    }
}

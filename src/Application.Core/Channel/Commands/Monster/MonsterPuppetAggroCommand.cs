using Application.Core.Game.Life;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MonsterPuppetAggroCommand: IWorldChannelCommand
    {
        Monster _mob;

        public MonsterPuppetAggroCommand(Monster mob)
        {
            _mob = mob;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.ApplyPuppetAggro();
        }
    }
}

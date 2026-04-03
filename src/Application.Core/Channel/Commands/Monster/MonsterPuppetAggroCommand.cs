using Application.Core.Game.Life;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MonsterPuppetAggroCommand: IWorldChannelCommand
    {
        public string Name => nameof(MonsterPuppetAggroCommand);
        Monster _mob;

        public MonsterPuppetAggroCommand(Monster mob)
        {
            _mob = mob;
        }

        public void Execute(WorldChannel ctx)
        {
            _mob.ApplyPuppetAggro();
        }
    }
}

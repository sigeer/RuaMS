using Application.Core.Game.Life;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MonsterFriendlyDropCommand : IWorldChannelCommand
    {
        readonly Monster _mob;

        public MonsterFriendlyDropCommand(Monster mob)
        {
            _mob = mob;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.FriendlyDrop();
        }
    }
}

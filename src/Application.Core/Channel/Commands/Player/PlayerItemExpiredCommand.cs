using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class PlayerItemExpiredCommand : IWorldChannelCommand
    {
        Player _chr;

        public PlayerItemExpiredCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ClearExpiredItems();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class PlayerChairBuffCommand : IWorldChannelCommand
    {
        Player _chr;

        public PlayerChairBuffCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ApplayChairBuff();
        }
    }
}

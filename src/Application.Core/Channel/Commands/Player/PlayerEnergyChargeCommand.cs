using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class PlayerEnergyChargeCommand : IWorldChannelCommand
    {
        Player _chr;

        public PlayerEnergyChargeCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ApplyEnergeCharge();
        }
    }
}

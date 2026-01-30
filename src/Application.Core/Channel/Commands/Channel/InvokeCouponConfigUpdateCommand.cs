using Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeCouponConfigUpdateCommand : IWorldChannelCommand
    {
        Config.CouponConfig _config;

        public InvokeCouponConfigUpdateCommand(CouponConfig config)
        {
            _config = config;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var chr in ctx.WorldChannel.getPlayerStorage().getAllCharacters())
            {
                chr.updateCouponRates();
            }
        }
    }
}

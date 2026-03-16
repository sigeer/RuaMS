using Application.Core.Channel.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeCheckMarriageCommand : IWorldChannelCommand
    {
        int _chrId;

        public InvokeCheckMarriageCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.Players.getCharacterById(_chrId);
            if (chr == null)
            {
                return;
            }

            var service = ctx.WorldChannel.NodeService.ServiceProvider.GetRequiredService<MarriageManager>();
            service.CheckMarriageData(chr);
        }
    }
}

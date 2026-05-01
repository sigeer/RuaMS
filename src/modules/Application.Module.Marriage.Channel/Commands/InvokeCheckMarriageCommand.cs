using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeCheckMarriageCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokeCheckMarriageCommand);

        int _chrId;

        public InvokeCheckMarriageCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(WorldChannel ctx)
        {
            var chr = ctx.Players.getCharacterById(_chrId);
            if (chr == null)
            {
                return;
            }

            var service = ctx.NodeService.ServiceProvider.GetRequiredService<MarriageManager>();
            service.CheckMarriageData(chr);
        }
    }
}

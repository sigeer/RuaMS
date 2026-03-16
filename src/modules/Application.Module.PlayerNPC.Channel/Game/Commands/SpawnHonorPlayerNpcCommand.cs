using Application.Core.Channel.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.PlayerNPC.Channel.Game.Commands
{
    internal class SpawnHonorPlayerNpcCommand : IWorldChannelCommand
    {

        int _chrId;

        public SpawnHonorPlayerNpcCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (chr != null && !chr.isGM())
            {
                ctx.WorldChannel.NodeService.ServiceProvider.GetRequiredService<PlayerNPCManager>().SpawnPlayerNPCByHonor(chr);
            }
        }
    }
}

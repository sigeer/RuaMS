using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Channel.Game.Commands
{
    internal class SpawnHonorPlayerNpcCommand : IWorldChannelCommand
    {

        public string Name => nameof(SpawnHonorPlayerNpcCommand);
        int _chrId;

        public SpawnHonorPlayerNpcCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(WorldChannel ctx)
        {
            var chr = ctx.getPlayerStorage().getCharacterById(_chrId);
            if (chr != null && !chr.isGM())
            {
                ctx.NodeService.ServiceProvider.GetRequiredService<PlayerNPCManager>().SpawnPlayerNPCByHonor(chr);
            }
        }
    }
}

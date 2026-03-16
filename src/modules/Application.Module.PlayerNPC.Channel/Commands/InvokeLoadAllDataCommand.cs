using Application.Core.Channel.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Channel.Commands
{
    internal class InvokeLoadAllDataCommand : IChannelCommand
    {
        public void Execute(ChannelNodeCommandContext ctx)
        {
            ctx.Server.ServiceProvider.GetRequiredService<PlayerNPCManager>().LoadAllData();
        }
    }
}

using Application.Core.Channel.HostExtensions;
using Application.Core.ServerTransports;
using Application.Module.Duey.Channel.InProgress;
using Application.Module.Maker.Channel.InProgress;
using Application.Module.PlayerNPC.Channel.InProgress;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {

        public static void AddChannelServerInProgress(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();
            builder.AddChannelServer();

            builder.Services.AddDueyInProgress();
            builder.Services.AddMakerInProgress();
            builder.Services.AddPlayerNPCInProgress();
        }
    }
}

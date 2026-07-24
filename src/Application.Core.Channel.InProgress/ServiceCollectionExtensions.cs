using Application.Core.Channel.HostExtensions;
using Application.Core.ServerTransports;
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

            // builder.Services.AddPlayerNPCInProgress();
        }
    }
}

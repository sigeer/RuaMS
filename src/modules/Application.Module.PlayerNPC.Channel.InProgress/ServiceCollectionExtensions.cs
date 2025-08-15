using Application.Module.PlayerNPC.Master;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayerNPCInProgress(this IServiceCollection services)
        {
            services.AddSingleton<IChannelTransport, LocalChannelServerTransport>();
            services.AddPlayerNPCChannel();
            return services;
        }
    }

}

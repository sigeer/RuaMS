using Application.Core.Channel.Events;
using Application.Core.Channel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Fishing.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFishing(this IServiceCollection services)
        {
            services.AddSingleton<IFishingService, FishingChannelModule>();
            services.AddSingleton<ChannelModule, FishingChannelModule>();
            services.AddSingleton<FishingManager>();

            return services;
        }
    }
}

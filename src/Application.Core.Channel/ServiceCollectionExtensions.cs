using Application.Core.Channel.Net;
using Application.Core.Game.TheWorld;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {

            services.AddSingleton<ChannelPacketProcessor>();
            return services;
        }
        public static IServiceCollection RegisterChannelServer(this IServiceCollection services)
        {
            services.AddHandlers();

            services.AddScoped<ChannelServerInitializer>();
            services.AddScoped<ChannelServer>();
            services.AddScoped<IWorldChannel, WorldChannel>();
            return services;
        }
    }
}

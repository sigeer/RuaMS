using Application.Core.ServerTransports;
using Application.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Local
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalServer(this IServiceCollection services)
        {
            services.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();
            services.AddSingleton<ChannelServerConfig>(new ChannelServerConfig());
            return services;
        }
    }
}

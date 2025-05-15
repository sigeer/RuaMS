using Application.Core.Channel.Net;
using Application.Core.DtoMappers;
using Application.Core.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChannelHandlers(this IServiceCollection services)
        {
            services.AddSingleton<ChannelPacketProcessor>();

            var interfaceType = typeof(IChannelHandler);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.Add(new ServiceDescriptor(interfaceType, impl, ServiceLifetime.Singleton));
            }
            return services;
        }
        public static IServiceCollection AddChannelServer(this IServiceCollection services)
        {
            services.AddChannelHandlers();

            services.AddAutoMapper(typeof(CharacterDtoMapper));
            return services;
        }
    }
}

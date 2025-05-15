using Application.Core.Login.Net;
using Application.Core.Login.Session;
using Application.Core.Net;
using Application.Core.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddLoginHandlers(this IServiceCollection services)
        {
            services.AddSingleton<LoginPacketProcessor>();

            var interfaceType = typeof(ILoginHandler);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.Add(new ServiceDescriptor(interfaceType, impl, ServiceLifetime.Singleton));
            }
            return services;
        }

        public static IServiceCollection AddLoginServer(this IServiceCollection services)
        {
            services.AddLoginHandlers();

            services.AddSingleton<SessionCoordinator>();
            services.AddSingleton<IMasterServer, MasterServer>();
            return services;
        }
    }
}

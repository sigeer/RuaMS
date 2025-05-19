using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Net;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Net;
using Application.Core.Servers;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;

namespace Application.Core.Login
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddLoginHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPacketProcessor<ILoginClient>, LoginPacketProcessor>();

            var interfaceType = typeof(LoginHandlerBase);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.AddSingleton(impl);
            }
            services.AddSingleton<KeepAliveHandler<ILoginClient>>();
            services.AddSingleton<CustomPacketHandler<ILoginClient>>();
            return services;
        }

        public static IServiceCollection AddSessionManager(this IServiceCollection services)
        {
            services.AddSingleton<SessionCoordinator>();
            services.AddSingleton<HostHwidCache>();
            services.AddSingleton<SessionDAO>();
            return services;
        }

        public static IServiceCollection AddLoginServer(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DtoMapper));

            services.AddLoginHandlers();

            services.AddSessionManager();

            services.AddSingleton<AccountManager>();
            services.AddSingleton<CharacterManager>();
            services.AddSingleton<CharacterService>();
            services.AddSingleton<StorageService>();

            services.AddSingleton<LoginService>();
            services.AddSingleton<IMasterServer, MasterServer>();
            return services;
        }
    }
}

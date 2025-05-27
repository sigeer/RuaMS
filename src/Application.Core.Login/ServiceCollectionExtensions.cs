using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Net;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Login.Tasks;
using Application.Core.Net;
using Application.Core.Servers;
using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;

namespace Application.Core.Login
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbFactory(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<DBContext>(o =>
            {
                o.UseMySQL(connectionString);
            });
            return services;
        }
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
            services.AddSingleton<HwidAssociationExpiry>();
            services.AddSingleton<LoginStorage>();
            services.AddSingleton<LoginBypassCoordinator>();
            services.AddSingleton<SessionInitialization>();
            return services;
        }

        static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.AddSingleton<DataStorage>();

            services.AddSingleton<StorageService>();
            return services;
        }

        static IServiceCollection AddScheduleTask(this IServiceCollection services)
        {
            services.AddSingleton<RankingLoginTask>();
            services.AddSingleton<DueyFredrickTask>();
            return services;
        }

        static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<AccountManager>();
            services.AddSingleton<CharacterManager>();

            services.AddSingleton<CharacterService>();
            services.AddSingleton<ServerService>();
            services.AddSingleton<LoginService>();
            services.AddSingleton<ItemService>();
            services.AddSingleton<DueyService>();
            services.AddSingleton<FredrickService>();
            services.AddSingleton<NoteService>();
            services.AddSingleton<ShopService>();
            return services;
        }

        public static IServiceCollection AddLoginServer(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DtoMapper));

            services.AddLoginHandlers();

            services.AddSessionManager();

            services.AddServices();
            services.AddStorage();
            services.AddSingleton<IMasterServer, MasterServer>();

            services.AddScheduleTask();
            return services;
        }
    }
}

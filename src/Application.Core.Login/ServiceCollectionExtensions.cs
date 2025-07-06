using Application.Core.Login.Client;
using Application.Core.Login.Mappers;
using Application.Core.Login.Models.Invitations;
using Application.Core.Login.Modules;
using Application.Core.Login.Net;
using Application.Core.Login.ServerData;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Login.Shared;
using Application.Core.Login.Tasks;
using Application.EF;
using Application.Shared.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.AddSingleton<RankingCommandTask>();
            services.AddSingleton<CouponTask>();
            return services;
        }

        static IServiceCollection AddInvitationService(this IServiceCollection services)
        {
            services.AddSingleton<InviteMasterHandlerRegistry>();
            services.AddSingleton<InvitationService>();

            services.AddSingleton<InviteMasterHandler, PartyInviteHandler>();
            services.AddSingleton<InviteMasterHandler, GuildInviteHandler>();
            services.AddSingleton<InviteMasterHandler, AllianceInviteHandler>();
            services.AddSingleton<InviteMasterHandler, MessengerInviteHandler>();
            return services;
        }

        static IServiceCollection AddDataManager(this IServiceCollection services)
        {
            services.AddSingleton<IStorage, ResourceDataManager>();
            services.AddSingleton<ResourceDataManager>();

            services.AddSingleton<IStorage, NewYearCardManager>();
            services.AddSingleton<NewYearCardManager>();

            return services;
        }

        static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<CharacterService>();
            services.AddSingleton<LoginService>();
            services.AddSingleton<ItemService>();
            services.AddSingleton<FredrickService>();
            services.AddSingleton<NoteService>();
            services.AddSingleton<ShopService>();
            services.AddSingleton<MessageService>();
            services.AddSingleton<RankService>();

            services.TryAddSingleton<IExpeditionService, DefaultExpeditionService>();
            services.AddInvitationService();
            return services;
        }

        public static IServiceCollection AddLoginServer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<DBContext>(o =>
            {
                o.UseMySQL(connectionString);
            });
            services.AddAutoMapper(typeof(ProtoMapper).Assembly);

            services.AddLoginHandlers();

            services.AddSessionManager();

            services.AddDataManager();
            services.AddServices();
            services.AddStorage();
            services.AddDistributedMemoryCache();
            services.AddScheduleTask();

            services.AddGrpc();
            services.AddSingleton<IServerBootstrap, DefaultMasterBootstrap>();
            services.AddSingleton<MasterServer>();
            services.AddHostedService<MasterHost>();
            return services;
        }
    }
}

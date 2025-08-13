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
using AutoMapper.Extensions.ExpressionMapping;
using Application.Core.Login.Datas;
using Microsoft.Extensions.Configuration;
using Application.Utility;
using Application.Protos;

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
            services.AddSingleton<NoteManager>();
            services.AddSingleton<IStorage, NoteManager>(sp => sp.GetRequiredService<NoteManager>());

            services.AddSingleton<ResourceDataManager>();
            services.AddSingleton<IStorage, ResourceDataManager>(sp => sp.GetRequiredService<ResourceDataManager>());

            services.AddSingleton<NewYearCardManager>();
            services.AddSingleton<IStorage, NewYearCardManager>(sp => sp.GetRequiredService<NewYearCardManager>());

            services.AddSingleton<GiftManager>();
            services.AddSingleton<IStorage, GiftManager>(sp => sp.GetRequiredService<GiftManager>());

            services.AddSingleton<RingManager>();
            services.AddSingleton<IStorage, RingManager>(sp => sp.GetRequiredService<RingManager>());

            services.AddSingleton<PlayerShopManager>();
            services.AddSingleton<IStorage, PlayerShopManager>(sp => sp.GetRequiredService<PlayerShopManager>());

            services.AddSingleton<AccountHistoryManager>();
            services.AddSingleton<IStorage, AccountHistoryManager>(sp => sp.GetRequiredService<AccountHistoryManager>());
            services.AddSingleton<AccountBanManager>();
            services.AddSingleton<IStorage, AccountBanManager>(sp => sp.GetRequiredService<AccountBanManager>());

            services.AddSingleton<GachaponManager>();
            services.AddSingleton<IStorage, GachaponManager>(sp => sp.GetRequiredService<GachaponManager>());

            services.AddSingleton<CDKManager>();
            services.AddSingleton<IStorage, CDKManager>(sp => sp.GetRequiredService<CDKManager>());

            services.AddSingleton<BuddyManager>();

            services.AddSingleton<InventoryManager>();
            services.AddSingleton<ItemFactoryManager>();
            services.AddSingleton<SystemManager>();

            services.AddSingleton<ServerManager>();
            services.AddSingleton<CouponManager>();
            services.AddSingleton<GuildManager>();
            services.AddSingleton<TeamManager>();
            services.AddSingleton<AccountManager>();
            services.AddSingleton<CharacterManager>();
            services.AddSingleton<BuffManager>();
            services.AddSingleton<ChatRoomManager>();
            services.AddSingleton<CashShopDataManager>();
            services.AddSingleton<InvitationManager>();
            return services;
        }

        static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<LoginService>();
            services.AddSingleton<ItemService>();

            services.AddSingleton<ShopService>();
            services.AddSingleton<MessageService>();
            services.AddSingleton<RankService>();

            services.TryAddSingleton<IExpeditionService, DefaultExpeditionService>();
            services.AddInvitationService();
            services.AddSingleton<CrossServerService>();
            return services;
        }

        public static IServiceCollection AddLoginServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<DBContext>(o =>
            {
                o.UseMySQL(configuration.GetConnectionString(AppSettingKeys.ConnectStr_Mysql)!);
            });
            services.AddAutoMapper(cfg =>
            {
                cfg.AddExpressionMapping();
            }, typeof(ProtoMapper).Assembly);

            services.AddLoginHandlers();

            services.AddSessionManager();

            services.AddDataManager();
            services.AddServices();
            services.AddStorage();
            services.AddDistributedMemoryCache();
            services.AddScheduleTask();

            if (configuration.GetValue<bool>(AppSettingKeys.AllowMultiMachine))
            {
                services.AddGrpc(options =>
                {
                    options.Interceptors.Add<LoggingInterceptor>();
                });
            }
 
            services.AddSingleton<IServerBootstrap, DefaultMasterBootstrap>();
            services.AddSingleton<MasterServer>();
            services.AddHostedService<MasterHost>();
            return services;
        }
    }
}

using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Infrastructures;
using Application.Core.Channel.Invitation;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Game.Commands;
using Application.Core.Mappers;
using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Core.Servers.Services;
using Application.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using net.server.handlers;

namespace Application.Core.Channel
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection AddInvitationService(this IServiceCollection services)
        {
            services.AddSingleton<InviteChannelHandlerRegistry>();

            services.AddSingleton<InviteChannelHandler, PartyInviteChannelHandler>();
            services.AddSingleton<InviteChannelHandler, GuildInviteChannelHandler>();
            services.AddSingleton<InviteChannelHandler, AllianceInviteChannelHandler>();
            services.AddSingleton<InviteChannelHandler, MessengerInviteChannelHandler>();
            return services;
        }

        private static IServiceCollection AddChannelHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPacketProcessor<IChannelClient>, ChannelPacketProcessor>();

            var interfaceType = typeof(ChannelHandlerBase);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.TryAddSingleton(impl);
            }
            services.AddSingleton<KeepAliveHandler<IChannelClient>>();
            services.AddSingleton<CustomPacketHandler<IChannelClient>>();
            return services;
        }

        private static IServiceCollection AddChannelCommands(this IServiceCollection services)
        {
            services.AddSingleton<CommandExecutor>();

            var interfaceType = typeof(CommandBase);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.AddSingleton(interfaceType, impl);
            }
            return services;
        }

        static IServiceCollection AddChannelService(this IServiceCollection services)
        {
            // 可能同一机器创建多个频道，wz资源读取使用单例
            services.AddSingleton<SkillbookInformationProvider>();
            services.AddSingleton<WZDataBootstrap, SkillbookInformationProvider>(sp => sp.GetRequiredService<SkillbookInformationProvider>());

            services.AddSingleton<CashItemProvider>();
            services.AddSingleton<WZDataBootstrap, CashItemProvider>(sp => sp.GetRequiredService<CashItemProvider>());

            services.AddSingleton<MonsterInformationProvider>();
            services.AddSingleton<IStaticService, MonsterInformationProvider>(sp => sp.GetRequiredService<MonsterInformationProvider>());
            services.AddSingleton<WZDataBootstrap, MonsterInformationProvider>(sp => sp.GetRequiredService<MonsterInformationProvider>());

            services.AddSingleton<ItemInformationProvider>();
            services.AddSingleton<IStaticService, ItemInformationProvider>(sp => sp.GetRequiredService<ItemInformationProvider>());
            services.AddSingleton<WZDataBootstrap, ItemInformationProvider>(sp => sp.GetRequiredService<ItemInformationProvider>());

            services.AddSingleton<ShopManager>();

            services.AddSingleton<ItemService>();
            services.AddSingleton<RankService>();

            // 频道的数据中心不再与频道关联，而是与频道所在的进程关联（同一进程多个频道）
            services.AddSingleton<TeamManager>();
            services.AddSingleton<GuildManager>();
            services.AddSingleton<ChatRoomService>();
            services.AddSingleton<ExpeditionService>();
            services.AddSingleton<NewYearCardService>();
            services.AddSingleton<NoteService>();
            services.AddSingleton<PlayerShopService>();
            services.TryAddSingleton<IFishingService, DefaultFishingService>();
            services.TryAddSingleton<IDueyService, DefaultDueyService>();
            services.TryAddSingleton<IItemDistributeService, DefaultItemDistributeService>();
            services.TryAddSingleton<IPlayerNPCService, DefaultPlayerNPCService>();

            services.AddSingleton<ItemTransactionService>();

            services.AddSingleton<MatchCheckerGuildCreationListener>();
            services.AddSingleton<MatchCheckerCPQChallengeListener>();

            services.AddInvitationService();

            services.AddMemoryCache();
            return services;
        }

        public static IServiceCollection AddChannelServer(this IServiceCollection services)
        {
            services.AddChannelCommands();
            services.AddChannelHandlers();

            services.AddSingleton<IServerBootstrap, DefaultChannelBootstrap>();

            services.AddOptions<ChannelServerConfig>().BindConfiguration("ChannelServerConfig");
            services.AddSingleton<WorldChannelServer>();
            services.AddChannelService();

            services.AddSingleton<DataService>();

            services.AddAutoMapper(typeof(ProtoMapper));
            services.AddHostedService<ChannelHost>();
            return services;
        }
    }
}

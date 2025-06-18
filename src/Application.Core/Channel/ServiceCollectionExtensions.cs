using Application.Core.Channel.Invitation;
using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Commands;
using Application.Core.Mappers;
using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Core.Servers.Services;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;
using server;

namespace Application.Core.Channel
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection AddInvitationService(this IServiceCollection services)
        {
            services.AddSingleton<InviteChannelHandlerRegistry>();

            services.AddSingleton<PartyInviteChannelHandler>();
            services.AddSingleton<GuildInviteChannelHandler>();
            services.AddSingleton<AllianceInviteChannelHandler>();
            services.AddSingleton<MessengerInviteChannelHandler>();
            return services;
        }

        private static IServiceCollection AddChannelHandlers(this IServiceCollection services)
        {
            services.AddScoped<IPacketProcessor<IChannelClient>, ChannelPacketProcessor>();

            var interfaceType = typeof(ChannelHandlerBase);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.AddScoped(impl);
            }
            services.AddScoped<KeepAliveHandler<IChannelClient>>();
            services.AddScoped<CustomPacketHandler<IChannelClient>>();
            return services;
        }

        private static IServiceCollection AddChannelCommands(this IServiceCollection services)
        {
            services.AddScoped<CommandExecutor>();

            var interfaceType = typeof(CommandBase);
            var implementations = interfaceType.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                services.AddScoped(interfaceType, impl);
            }
            return services;
        }

        static IServiceCollection AddChannelService(this IServiceCollection services)
        {
            // 可能同一机器创建多个频道，wz资源读取使用单例
            services.AddSingleton<SkillbookInformationProvider>();
            services.AddSingleton<ShopFactory>();

            services.AddSingleton<ItemService>();
            services.AddSingleton<RankService>();

            // 频道的数据中心不再与频道关联，而是与频道所在的进程关联（同一进程多个频道）
            services.AddSingleton<TeamManager>();
            services.AddSingleton<GuildManager>();
            services.AddSingleton<ChatRoomService>();

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

            services.AddChannelService();

            services.AddScoped<CharacterService>();

            services.AddAutoMapper(typeof(ProtoMapper));
            return services;
        }
    }
}

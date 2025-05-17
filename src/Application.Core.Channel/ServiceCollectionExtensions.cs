using Application.Core.Channel.Mappers;
using Application.Core.Channel.Net;
using Application.Core.Channel.Services;
using Application.Core.Game.Commands;
using Application.Core.Game.Commands.Gm6;
using Application.Core.Net;
using client.processor.npc;
using database.note;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;
using scripting.map;
using scripting.npc;
using scripting.portal;
using scripting.quest;
using scripting.reactor;
using service;

namespace Application.Core.Channel
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddScriptManagers(this IServiceCollection services)
        {
            services.AddScoped<NPCScriptManager>();
            services.AddScoped<MapScriptManager>();
            services.AddScoped<PortalScriptManager>();
            services.AddScoped<QuestScriptManager>();
            services.AddScoped<ReactorScriptManager>();
            services.AddScoped<DevtestScriptManager>();
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
        public static IServiceCollection AddChannelServer(this IServiceCollection services)
        {
            services.AddChannelCommands();
            services.AddChannelHandlers();

            services.AddScriptManagers();

            services.AddSingleton<NoteDao>();
            services.AddSingleton<NoteService>();
            services.AddSingleton<FredrickProcessor>();

            services.AddScoped<CharacterService>();

            services.AddAutoMapper(typeof(ObjectMapper));
            return services;
        }
    }
}

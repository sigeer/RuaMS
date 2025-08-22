using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Module.Duey.Channel.Models;
using Application.Module.Duey.Channel.Net.Handlers;
using Application.Shared.Net;
using Application.Shared.Servers;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Application.Module.Duey.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyChannel(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(typeof(Mapper).Assembly);
            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();

            services.AddSingleton<DueyManager>();
            services.AddSingleton<ChannelModule, DueyChannelModule>();
            services.AddSingleton<IDueyService, DueyChannelModule>();
            services.AddSingleton<IItemDistributeService, DueyDistributeService>();

            services.AddSingleton<DueyHandler>();
            services.AddSingleton<IServerBootstrap, DueyChannelBootstrap>();
            return services;
        }

        public static void UseDuey(this IHost app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.DUEY_ACTION, app.Services.GetRequiredService<DueyHandler>());
        }
    }

    public class DueyChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            app.UseDuey();
        }
    }
}

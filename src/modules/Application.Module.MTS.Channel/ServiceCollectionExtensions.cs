using Application.Core.Client;
using Application.Module.MTS.Channel.Net.Handlers;
using Application.Shared.Net;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Module.MTS.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildBBSChannel(this IServiceCollection services)
        {
            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();
            services.AddSingleton<MTSManager>();

            services.AddSingleton<MTSHandler>();
            services.AddSingleton<IServerBootstrap, MTSChannelBootstrap>();
            return services;
        }
    }

    public class MTSChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.MTS_OPERATION, app.Services.GetRequiredService<MTSHandler>());
        }
    }
}

using Application.Core.Client;
using Application.Module.BBS.Channel.Net.Handlers;
using Application.Shared.Net;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Module.BBS.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildBBSChannel(this IServiceCollection services)
        {
            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();
            services.AddSingleton<BBSManager>();

            services.AddSingleton<BBSOperationHandler>();
            services.AddSingleton<IServerBootstrap, BBSChannelBootstrap>();
            return services;
        }
    }

    public class BBSChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.BBS_OPERATION, app.Services.GetRequiredService<BBSOperationHandler>());
        }
    }
}

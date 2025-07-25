using Application.Core.Client;
using Application.Module.Marriage.Channel.Net.Handlers;
using Application.Module.Marriage.Channel.Scripting;
using Application.Scripting;
using Application.Shared.Net;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Marriage.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarriageChannel(this IServiceCollection services)
        {
            // services.TryAddSingleton<IChannelServerTransport, DefaultChannelTransport>();
            services.AddSingleton<WeddingManager>();

            services.AddSingleton<RingActionHandler>();
            services.AddSingleton<IServerBootstrap, MarriageChannelBootstrap>();
            services.AddSingleton<IAddtionalRegistry, MarriageScriptRegitry>();
            return services;
        }
    }

    public class MarriageChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.RING_ACTION, app.Services.GetRequiredService<RingActionHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.WEDDING_ACTION, app.Services.GetRequiredService<WeddingHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.WEDDING_TALK, app.Services.GetRequiredService<WeddingTalkHandler>());
            // channelPacketProcessor.TryAddHandler((short)RecvOpcode.WEDDING_TALK_MORE, app.Services.GetRequiredService<WeddingTalkMoreHandler>());
        }
    }
}

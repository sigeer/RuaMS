using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Core.ServerTransports;
using Application.Module.Marriage.Channel.Models;
using Application.Module.Marriage.Channel.Net.Handlers;
using Application.Module.Marriage.Channel.Scripting;
using Application.Scripting;
using Application.Shared.Net;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Module.Marriage.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarriageChannel(this IServiceCollection services)
        {
            services.AddGrpcClient<MarriageServiceProto.ChannelService.ChannelServiceClient>("MarriageGrpcClient", (sp, o) =>
            {
                o.Address = new(AppSettingKeys.Grpc_Master);
            }).AddInterceptor<WithServerNameInterceptor>();

            services.AddAutoMapper(typeof(Mapper));
            services.TryAddSingleton<IChannelServerTransport, DefaultChannelServerTransport>();

            services.AddSingleton<MarriageManager>();
            services.AddSingleton<WeddingManager>();

            services.AddSingleton<MarriageChannelModule>();
            services.AddSingleton<IMarriageService, MarriageChannelModule>(sp => sp.GetRequiredService<MarriageChannelModule>());
            services.AddSingleton<AbstractChannelModule, MarriageChannelModule>(sp => sp.GetRequiredService<MarriageChannelModule>());

            services.AddSingleton<RingActionHandler>();
            services.AddSingleton<WeddingHandler>();
            services.AddSingleton<WeddingTalkHandler>();
            services.AddSingleton<WeddingTalkMoreHandler>();
            services.AddSingleton<SpouseChatHandler>();
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
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.SPOUSE_CHAT, app.Services.GetRequiredService<SpouseChatHandler>());
            // channelPacketProcessor.TryAddHandler((short)RecvOpcode.WEDDING_TALK_MORE, app.Services.GetRequiredService<WeddingTalkMoreHandler>());
        }
    }
}

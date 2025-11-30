using Application.Core.Channel.Modules;
using Application.Core.Client;
using Application.Module.Family.Channel.Net.Handlers;
using Application.Module.Family.Common;
using Application.Shared.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Module.Family.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFamilySystem(this IServiceCollection services)
        {
            services.AddSingleton<FamilyManager>();
            services.AddOptions<FamilyConfigs>();

            services.AddSingleton<AbstractChannelModule, ChannelFamilyModule>();

            services.AddSingleton<OpenFamilyHandler>();
            services.AddSingleton<OpenFamilyPedigreeHandler>();
            services.AddSingleton<FamilyAddHandler>();
            services.AddSingleton<FamilySeparateHandler>();
            services.AddSingleton<FamilyUseHandler>();
            services.AddSingleton<FamilyPreceptsHandler>();
            services.AddSingleton<FamilySummonResponseHandler>();
            services.AddSingleton<AcceptFamilyHandler>();

            return services;
        }
        public static void UseFamily(this IHost app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.OPEN_FAMILY, app.Services.GetRequiredService<OpenFamilyHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.OPEN_FAMILY_PEDIGREE, app.Services.GetRequiredService<OpenFamilyPedigreeHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.ADD_FAMILY, app.Services.GetRequiredService<FamilyAddHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.SEPARATE_FAMILY_BY_SENIOR, app.Services.GetRequiredService<FamilySeparateHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.SEPARATE_FAMILY_BY_JUNIOR, app.Services.GetRequiredService<FamilySeparateHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.USE_FAMILY, app.Services.GetRequiredService<FamilyUseHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.CHANGE_FAMILY_MESSAGE, app.Services.GetRequiredService<FamilyPreceptsHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.FAMILY_SUMMON_RESPONSE, app.Services.GetRequiredService<FamilySummonResponseHandler>());
            channelPacketProcessor.TryAddHandler((short)RecvOpcode.ACCEPT_FAMILY, app.Services.GetRequiredService<AcceptFamilyHandler>());
        }

    }
}

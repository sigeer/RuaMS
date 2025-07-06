using Application.Core.Channel.Modules;
using Application.Core.Client;
using Application.Module.Maker.Channel.Net.Handlers;
using Application.Shared.Net;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Module.Maker.Channel
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Maker应该是个未完成品--有wz，但是代码中的数据来源于数据库，没有用到表（makerrewarddata）
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMakerChannel(this IServiceCollection services)
        {
            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();
            services.AddSingleton<MakerManager>();
            services.AddSingleton<ChannelModule, MakerChannelModule>();

            services.AddSingleton<MakerSkillHandler>();
            services.AddSingleton<IServerBootstrap, MakerChannelBootstrap>();
            return services;
        }
    }

    public class MakerChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.MAKER_SKILL, app.Services.GetRequiredService<MakerSkillHandler>());
        }
    }
}

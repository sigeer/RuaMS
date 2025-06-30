using Application.Core.Channel.Events;
using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Module.Duey.Channel.Models;
using Application.Module.Duey.Channel.Net.Handlers;
using Application.Shared.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Application.Module.Duey.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyChannel(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();

            services.AddSingleton<DueyManager>();
            services.AddSingleton<ChannelModule, DueyChannelModule>();
            services.AddSingleton<IItemDistributeService, DueyDistributeService>();

            services.AddSingleton<DueyHandler>();

            return services;
        }

        public static void UseDuey(this IHost app)
        {
            var channelPacketProcessor = app.Services.GetRequiredService<IPacketProcessor<IChannelClient>>();

            channelPacketProcessor.TryAddHandler((short)RecvOpcode.DUEY_ACTION, app.Services.GetRequiredService<DueyHandler>());
        }
    }
}

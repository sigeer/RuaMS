using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Game.Commands;
using Application.Core.ServerTransports;
using Application.Module.PlayerNPC.Channel.Commands.Gm4;
using Application.Module.PlayerNPC.Channel.Commands.Gm6;
using Application.Module.PlayerNPC.Channel.Models;
using Application.Module.PlayerNPC.Common;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Module.PlayerNPC.Channel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayerNPCChannel(this IServiceCollection services)
        {
            var urlString = "http://_grpc.ruams-master";
            services.AddGrpcClient<PlayerNPCProto.ChannelService.ChannelServiceClient>((sp, o) =>
            {
                o.Address = new(urlString);
            }).AddInterceptor<WithServerNameInterceptor>();

            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<PlayerNPCChannelModule>();
            services.AddSingleton<AbstractChannelModule, PlayerNPCChannelModule>(sp => sp.GetRequiredService<PlayerNPCChannelModule>());
            services.AddSingleton<IPlayerNPCService, PlayerNPCChannelModule>(sp => sp.GetRequiredService<PlayerNPCChannelModule>());

            services.AddOptions<Configs>()
                .BindConfiguration("PlayerNpc");

            services.AddSingleton<PlayerNPCManager>();
            services.AddSingleton<IServerBootstrap, PlayerNPCChannelBootstrap>();

            services.AddSingleton<PlayerNpcCommand>();
            services.AddSingleton<PlayerNpcRemoveCommand>();
            services.AddSingleton<EraseAllPNpcsCommand>();

            services.TryAddSingleton<IChannelTransport, DefaultChannelTransport>();
            return services;
        }
    }

    public class PlayerNPCChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var executor = app.Services.GetRequiredService<CommandExecutor>();
            executor.TryRegisterCommand(app.Services.GetRequiredService<PlayerNpcCommand>());
            executor.TryRegisterCommand(app.Services.GetRequiredService<PlayerNpcRemoveCommand>());
            executor.TryRegisterCommand(app.Services.GetRequiredService<EraseAllPNpcsCommand>());
        }
    }
}

using Application.Core.Login.Events;
using Application.Core.Login.Shared;
using Application.Module.PlayerNPC.Master.Models;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayerNPCMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<IServerBootstrap, PlayerNPCMasterBootstrap>();

            services.AddSingleton<PlayerNPCManager>();
            services.AddSingleton<IStorage, PlayerNPCManager>(sp => sp.GetRequiredService<PlayerNPCManager>());
            services.AddSingleton<MasterTransport>();
            services.AddSingleton<MasterModule, PlayerNPCMasterModule>();
            return services;
        }

        public class PlayerNPCMasterBootstrap : IServerBootstrap
        {
            public void ConfigureHost(WebApplication app)
            {
                if (app.Configuration.GetValue<bool>(AppSettingKeys.AllowMultiMachine))
                {
                    app.MapGrpcService<GrpcServer>();
                }
            }
        }
    }

}

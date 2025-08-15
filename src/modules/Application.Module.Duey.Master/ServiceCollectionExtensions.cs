using Application.Core.Login.Events;
using Application.Core.Login.Shared;
using Application.Module.Duey.Master.Models;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Duey.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<IServerBootstrap, DueyMasterBootstrap>();

            services.AddSingleton<DueyMasterTransport>();
            services.AddSingleton<DueyManager>();
            services.AddSingleton<IStorage, DueyManager>(sp => sp.GetRequiredService<DueyManager>());
            services.AddSingleton<DueyTask>();
            services.AddSingleton<MasterModule, DueyMasterModule>();
            return services;
        }

    }

    public class DueyMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            if (app.Configuration.GetValue<bool>(AppSettingKeys.UseExtraChannel))
                app.MapGrpcService<DueyGrpcServer>();
        }
    }
}

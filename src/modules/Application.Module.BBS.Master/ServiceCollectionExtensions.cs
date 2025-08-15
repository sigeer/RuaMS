using Application.Core.Login.Shared;
using Application.Module.BBS.Master.Models;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.BBS.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildBBSMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<BBSManager>();
            services.AddSingleton<IStorage, BBSManager>(sp => sp.GetRequiredService<BBSManager>());
            services.AddSingleton<IServerBootstrap, BBSMasterBootstrap>();
            return services;
        }
    }

    public class BBSMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            if (app.Configuration.GetValue<bool>(AppSettingKeys.UseExtraChannel))
                app.MapGrpcService<GrpcService>();
        }
    }
}

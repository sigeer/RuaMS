using Application.Core.Login.Modules;
using Application.Core.Login.Shared;
using Application.Module.Marriage.Master.Models;
using Application.Shared.ServerExtensions;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Marriage.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarriageMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<MarriageManager>();
            services.AddSingleton<IStorage, MarriageManager>(sp => sp.GetRequiredService<MarriageManager>());

            services.AddSingleton<WeddingManager>();
            services.AddSingleton<MasterTransport>();
            services.AddSingleton<AbstractMasterModule, MarriageMasterModule>();

            services.AddSingleton<IServerBootstrap, MarriageMasterBootstrap>();
            return services;
        }
    }

    public class MarriageMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            if (app.Configuration.UseExtralChannel())
                app.MapGrpcService<GrpcService>();

        }
    }
}

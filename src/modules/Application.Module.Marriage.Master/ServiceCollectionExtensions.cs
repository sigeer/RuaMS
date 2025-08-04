using Application.Core.Login.Events;
using Application.Core.Login.Shared;
using Application.Module.Marriage.Master.Models;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
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
            services.AddSingleton<MasterModule, MarriageMasterModule>();

            services.AddSingleton<IServerBootstrap, MarriageMasterBootstrap>();
            return services;
        }
    }

    public class MarriageMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            app.MapGrpcService<GrpcService>();
        }
    }
}

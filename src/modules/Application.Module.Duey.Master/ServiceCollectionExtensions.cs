using Application.Core.Login.Events;
using Application.Core.Login.Modules;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Duey.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyMaster(this IServiceCollection services)
        {
            services.AddSingleton<IServerBootstrap, DueyMasterBootstrap>();

            services.AddSingleton<DueyMasterTransport>();
            services.AddSingleton<DueyManager>();
            services.AddSingleton<DueyTask>();
            services.AddSingleton<MasterModule, DueyMasterModule>();

            services.AddGrpc();
            return services;
        }

    }

    public class DueyMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            app.MapGrpcService<DueyGrpcServer>();
        }
    }
}

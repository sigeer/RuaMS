using Application.Core.Login.Events;
using Application.Module.Maker.Master.Models;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Maker.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMakerMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<MakerManager>();
            services.AddSingleton<MasterModule, MakerMasterModule>();
            services.AddSingleton<IServerBootstrap, MakerMasterBootstrap>();
            return services;
        }
    }

    public class MakerMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            app.MapGrpcService<GrpcServer>();
        }
    }
}

using Application.Core.Login.Modules;
using Application.Module.Maker.Master.Models;
using Application.Shared.ServerExtensions;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Maker.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMakerMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<MakerManager>();
            services.AddSingleton<AbstractMasterModule, MakerMasterModule>();
            services.AddSingleton<IServerBootstrap, MakerMasterBootstrap>();
            return services;
        }
    }

    public class MakerMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            if (app.Configuration.UseExtralChannel())
                app.MapGrpcService<GrpcServer>();
        }
    }
}

using Application.Core.Login.Events;
using Application.Core.Login.Shared;
using Application.Module.PlayerNPC.Master.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayerNPCMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<PlayerNPCManager>();
            services.AddSingleton<IStorage, PlayerNPCManager>(sp => sp.GetRequiredService<PlayerNPCManager>());
            services.AddSingleton<MasterTransport>();
            services.AddSingleton<MasterModule, PlayerNPCMasterModule>();
            return services;
        }
    }

}

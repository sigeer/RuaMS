using Application.Core.Login.Modules;
using Application.Core.Login.ServerData;
using Application.Core.Login.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayerNPCMaster(this IServiceCollection services)
        {
            services.AddSingleton<PlayerNPCManager>();
            services.AddSingleton<IPlayerNPCManager, PlayerNPCManager>(sp => sp.GetRequiredService<PlayerNPCManager>());
            services.AddSingleton<IStorage, PlayerNPCManager>(sp => sp.GetRequiredService<PlayerNPCManager>());

            services.AddSingleton<MasterTransport>();
            services.AddSingleton<AbstractMasterModule, PlayerNPCMasterModule>();
            return services;
        }
    }

}

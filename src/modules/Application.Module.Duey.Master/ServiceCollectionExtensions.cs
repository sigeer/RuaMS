using Application.Core.Login.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Duey.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyMaster(this IServiceCollection services)
        {
            services.AddSingleton<DueyMasterTransport>();
            services.AddSingleton<DueyManager>();
            services.AddSingleton<DueyTask>();
            services.AddSingleton<MasterModule, DueyMasterModule>();

            return services;
        }
    }
}

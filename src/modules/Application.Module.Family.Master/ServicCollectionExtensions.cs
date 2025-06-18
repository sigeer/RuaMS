using Application.Core.Login.Events;
using Application.Module.Family.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Family.Master
{
    public static class ServicCollectionExtensions
    {
        public static IServiceCollection AddFamilySystem(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddOptions<FamilyConfigs>().Bind(configuration);

            services.AddSingleton<FamilyManager>();
            services.AddSingleton<DataService>();

            services.AddSingleton<MasterModule, MasterFamilyModule>();

            return services;
        }

    }
}

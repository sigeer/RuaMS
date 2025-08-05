using Application.Core.Login.Events;
using Application.Core.Login.Shared;
using Application.Module.Family.Common;
using Application.Module.Family.Master.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Family.Master
{
    public static class ServicCollectionExtensions
    {
        public static IServiceCollection AddFamilySystem(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddOptions<FamilyConfigs>().Bind(configuration);
            services.AddAutoMapper(typeof(Mapper));

            services.AddSingleton<FamilyManager>();
            services.AddSingleton<IStorage, FamilyManager>(sp => sp.GetRequiredService<FamilyManager>());

            services.AddSingleton<MasterModule, MasterFamilyModule>();

            return services;
        }

    }
}

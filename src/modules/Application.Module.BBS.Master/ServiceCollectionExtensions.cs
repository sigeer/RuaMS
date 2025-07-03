using Application.Module.BBS.Master.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.BBS.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildBBSMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<BBSManager>();
            return services;
        }
    }
}

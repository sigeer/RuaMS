using Application.Module.Maker.Master.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Maker.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMakerMaster(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mapper));
            services.AddSingleton<MakerManager>();
            return services;
        }
    }
}

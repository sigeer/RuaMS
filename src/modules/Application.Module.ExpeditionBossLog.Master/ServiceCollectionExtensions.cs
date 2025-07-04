using Application.Core.Login.Events;
using Application.Core.Login.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.ExpeditionBossLog.Master
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExpeditionBossLogMaster(this IServiceCollection services)
        {
            services.AddSingleton<ExpeditionBossLogManager>();
            services.AddSingleton<IExpeditionService, ExpeditionLogModule>();
            services.AddSingleton<MasterModule, ExpeditionLogModule>();
            return services;
        }
    }
}

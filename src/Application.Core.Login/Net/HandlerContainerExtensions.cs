using Application.Core.Login.Net.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Net
{
    public static class HandlerContainerExtensions
    {
        public static IServiceCollection RegisterHandlers(this IServiceCollection services)
        {
            services.AddSingleton<LoginPasswordHandler>();
            services.AddSingleton<CharlistRequestHandler>();
            return services;
        }
    }
}

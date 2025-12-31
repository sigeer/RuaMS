using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Core.Login.Internal
{
    public static class Extension
    {
        public static IServiceCollection AddInternalSessionHandlers(this IServiceCollection services)
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in assemblyTypes)
            {
                if (type.IsClass && !type.IsAbstract && typeof(IInternalSessionMasterHandler).IsAssignableFrom(type))
                {
                    services.AddSingleton(type);
                }
            }
            return services;
        }
    }
}

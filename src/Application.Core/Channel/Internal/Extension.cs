using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Core.Channel.Internal
{
    public static class Extension
    {
        public static IServiceCollection AddInternalSessionHandlers(this IServiceCollection services)
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in assemblyTypes)
            {
                // 检查类型是否是非抽象的类，并且实现了IMyInterface接口
                if (type.IsClass && !type.IsAbstract && typeof(IInternalSessionChannelHandler).IsAssignableFrom(type))
                {
                    services.AddSingleton(type);
                }
            }
            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Duey.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDueyInProgress(this IServiceCollection services)
        {
            services.AddSingleton<IChannelTransport, LocalDueyChannelTransport>();
            services.AddDueyChannel();

            return services;
        }
    }
}

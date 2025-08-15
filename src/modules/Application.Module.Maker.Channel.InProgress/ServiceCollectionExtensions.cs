using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Maker.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMakerInProgress(this IServiceCollection services)
        {
            services.AddSingleton<IChannelTransport, LocalChannelTransport>();
            services.AddMakerChannel();

            return services;
        }
    }
}

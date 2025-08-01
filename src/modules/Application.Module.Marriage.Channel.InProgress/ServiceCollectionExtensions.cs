using Application.Module.Marriage.Master;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.Marriage.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarriageInProgress(this IServiceCollection services)
        {
            services.AddSingleton<IChannelServerTransport, LocalChannelTransport>();
            services.AddMarriageChannel();
            services.AddMarriageMaster();

            return services;
        }
    }
}

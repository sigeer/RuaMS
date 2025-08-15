using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.BBS.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildBBSInProgress(this IServiceCollection services)
        {
            services.AddSingleton<IChannelTransport, LocalChannelTransport>();
            services.AddGuildBBSChannel();

            return services;
        }
    }
}

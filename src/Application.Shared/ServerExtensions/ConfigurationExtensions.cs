using Application.Utility;
using Microsoft.Extensions.Configuration;

namespace Application.Shared.ServerExtensions
{
    public static class ConfigurationExtensions
    {
        public static bool UseExtralChannel(this IConfiguration configuration)
        {
            return configuration.GetSection(AppSettingKeys.GrpcEndpoint).Exists();
        }
    }
}

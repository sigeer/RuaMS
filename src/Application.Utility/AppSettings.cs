using Application.Utility.Configs;

namespace Application.Utility
{
    public class AppSettings
    {
        public readonly static string GetConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING") ?? YamlConfig.config.server.DB_CONNECTIONSTRING;
    }
}

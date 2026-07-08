using Application.Templates.Reader;
using Application.Templates.Reader.Img;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Xml;

namespace Application.Core.Channel.HostExtensions
{
    /// <summary>
    /// wz, script 资源设置
    /// </summary>
    public static class DataSourceExtensions
    {
        public static void AddDataSource(this WebApplicationBuilder builder)
        {
#if DEBUG
            if (builder.Environment.IsDevelopment())
            {
                var debugConfig = new Dictionary<string, string?>
                {
                    [$"{AppSettingKeys.Section_WZ}:BaseDir"] = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Application.Resources", "wz")),
                    [$"{AppSettingKeys.Section_Script}:BaseDir"] = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Application.Resources", "scripts")),
                };
                builder.Configuration.AddInMemoryCollection(debugConfig);
            }
#endif
        }

        public static void UseDataSource(this WebApplication app)
        {
            ScriptSource.Instance = new ScriptSource(app.Configuration.GetSection(AppSettingKeys.Section_Script));
            var wzDir = app.Configuration.GetSection(AppSettingKeys.Section_WZ).GetValue("BaseDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz"));
            var file = Directory.EnumerateFiles(wzDir, "*", SearchOption.AllDirectories).FirstOrDefault();
            if (file == null)
            {
                throw new Templates.Exceptions.DataDirNotFoundException();
            }

            ProviderSource providerSource;

            if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new Application.Templates.Reader.Xml.ServerXmlResolver(wzDir);
                providerSource = new ProviderSource(resolver);
                Application.Templates.Reader.Xml.Registor.Register(providerSource);
            }
            else if (file.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new Application.Templates.Reader.Img.ImgPathResolver(wzDir);
                providerSource = new ProviderSource(resolver);
                Application.Templates.Reader.Img.Registor.Register(providerSource);
            }
            else
            {
                throw new Templates.Exceptions.DataDirNotFoundException();
            }

            ProviderSource.Instance = providerSource;

            ProviderSource.Instance.UseLogger(app.Logger);
            ProviderSource.Instance.Debug();
        }
    }


}

using Application.Resources.Messages;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Globalization;

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

            ProviderSource.Instance = new ProviderSource(app.Configuration.GetSection(AppSettingKeys.Section_WZ));
            ProviderSource.Instance.RegisterProvider<MapProvider>(o => new MapProvider(o))
                .RegisterProvider<ReactorProvider>(o => { o.UseCache = false; return new ReactorProvider(o); })
                .RegisterProvider<QuestProvider>(o => new QuestProvider(o))
                .RegisterProvider<EquipProvider>(o => new EquipProvider(o))
                .RegisterProvider<ItemProvider>(o => new ItemProvider(o))
                .RegisterProvider<MobSkillProvider>(o => { o.UseCache = false; return new MobSkillProvider(o); })
                .RegisterProvider<EtcNpcLocationProvider>(o => new EtcNpcLocationProvider(o))
                .RegisterProvider<SkillProvider>(o => { o.UseCache = false; return new SkillProvider(o); })
                .RegisterProvider<MobWithBossHpBarProvider>(o => { o.UseCache = false; return new MobWithBossHpBarProvider(o); })

                .RegisterKeydProvider("zh-CN", o => new StringProvider(o, CultureInfo.GetCultureInfo("zh-CN")))
                .RegisterKeydProvider("en-US", o => new StringProvider(o, CultureInfo.GetCultureInfo("en-US")))
                .UseLogger(app.Logger);

            ProviderSource.Instance.Debug();
        }
    }


}

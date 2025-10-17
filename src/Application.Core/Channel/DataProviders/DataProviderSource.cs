using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using System.Globalization;

namespace Application.Core.Channel.DataProviders
{
    public class DataProviderSource
    {
        public static void Initialize()
        {
            ProviderFactory.Configure(option =>
            {
#if DEBUG
                // debug 时默认使用自带wz
                option.DataDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Application.Resources", "wz"));
#endif

                option.RegisterProvider<MapProvider>(() => new MapProvider(new Templates.TemplateOptions()));
                option.RegisterProvider<ReactorProvider>(() => new ReactorProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider<QuestProvider>(() => new QuestProvider(new Templates.TemplateOptions()));
                option.RegisterProvider<EquipProvider>(() => new EquipProvider(new Templates.TemplateOptions()));
                option.RegisterProvider<ItemProvider>(() => new ItemProvider(new Templates.TemplateOptions()));
                option.RegisterProvider<MobSkillProvider>(() => new MobSkillProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider<EtcNpcLocationProvider>(() => new EtcNpcLocationProvider(new Templates.TemplateOptions()));

                option.RegisterKeydProvider("zh-CN", () => new StringProvider(new Templates.TemplateOptions(), CultureInfo.GetCultureInfo("zh-CN")));
                option.RegisterKeydProvider("en-US", () => new StringProvider(new Templates.TemplateOptions(), CultureInfo.GetCultureInfo("en-US")));
            });
        }

        public static void ResetPath(string pathDir)
        {
            ProviderFactory.ConfigureWith(o => o.DataDir = pathDir);
        }
    }


}

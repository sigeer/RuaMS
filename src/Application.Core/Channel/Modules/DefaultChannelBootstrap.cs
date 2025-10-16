using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Shared.Servers;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using net.server.coordinator.matchchecker;
using System.Globalization;

namespace Application.Core.Channel.Modules
{
    public class DefaultChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            MatchCheckerStaticFactory.Context = new MatchCheckerStaticFactory(
                    app.Services.GetRequiredService<MatchCheckerGuildCreationListener>(),
                    app.Services.GetRequiredService<MatchCheckerCPQChallengeListener>());

            ProviderFactory.Configure(option =>
            {
#if DEBUG
                // debug 时默认使用自带wz
                option.DataDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Application.Resources", "wz"));
#endif

                option.RegisterProvider(new MapProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new ReactorProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new QuestProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new EquipProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new ItemProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new MobSkillProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new EtcNpcLocationProvider(new Templates.TemplateOptions()));

                option.RegisterKeydProvider("zh-CN", new StringProvider(new Templates.TemplateOptions(), CultureInfo.GetCultureInfo("zh-CN")));
                option.RegisterKeydProvider("en-US", new StringProvider(new Templates.TemplateOptions(), CultureInfo.GetCultureInfo("en-US")));
                option.UseLogger(app.Logger);
            });

            var staticServices = app.Services.GetServices<IStaticService>();
            foreach (var srv in staticServices)
            {
                srv.Register(app.Services);
            }
        }
    }
}

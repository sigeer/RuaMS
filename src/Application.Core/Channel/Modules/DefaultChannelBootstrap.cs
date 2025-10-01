using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Shared.Servers;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using net.server.coordinator.matchchecker;

namespace Application.Core.Channel.Modules
{
    public class DefaultChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            // Environment.SetEnvironmentVariable("ms-wz", "C:\\Demo\\MS\\wz");

            MatchCheckerStaticFactory.Context = new MatchCheckerStaticFactory(
                    app.Services.GetRequiredService<MatchCheckerGuildCreationListener>(),
                    app.Services.GetRequiredService<MatchCheckerCPQChallengeListener>());

            ProviderFactory.Initilaize(option =>
            {
                option.RegisterProvider(new MapProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new ReactorProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new QuestProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new EquipProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new ItemProvider(new Templates.TemplateOptions()));
                option.RegisterProvider(new MobSkillProvider(new Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new StringProvider(new Templates.TemplateOptions()));
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

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

            var staticServices = app.Services.GetServices<IStaticService>();
            foreach (var srv in staticServices)
            {
                srv.Register(app.Services);
            }
        }
    }
}

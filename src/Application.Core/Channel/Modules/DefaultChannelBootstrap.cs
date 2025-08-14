using Application.Core.Channel.Infrastructures;
using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using net.server.coordinator.matchchecker;

namespace Application.Core.Channel.Modules
{
    public class DefaultChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            // Environment.SetEnvironmentVariable("ms-wz", "D:\\Cosmic\\wz");

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

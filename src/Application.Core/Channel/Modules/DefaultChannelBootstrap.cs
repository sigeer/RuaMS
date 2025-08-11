using Application.Core.Channel.Infrastructures;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Modules
{
    public class DefaultChannelBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            var staticServices = app.Services.GetServices<IStaticService>();
            foreach (var srv in staticServices)
            {
                srv.Register(app.Services);
            }
        }
    }
}

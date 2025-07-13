using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Application.Shared.Servers
{
    public interface IServerBootstrap
    {
        void ConfigureHost(WebApplication app);
    }

}

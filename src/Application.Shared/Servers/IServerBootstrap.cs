using Microsoft.AspNetCore.Builder;

namespace Application.Shared.Servers
{
    public interface IServerBootstrap
    {
        void ConfigureHost(WebApplication app);
    }

}

using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;

namespace Application.Core.Login.Modules
{

    public class DefaultMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            // TODO: 在这里启动grcp server
        }
    }
}

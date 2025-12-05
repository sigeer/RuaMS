using Application.Core.Login.Servers;
using Application.Shared.ServerExtensions;
using Application.Shared.Servers;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Application.Core.Login.Modules
{

    public class DefaultMasterBootstrap : IServerBootstrap
    {
        public void ConfigureHost(WebApplication app)
        {
            // TODO: 在这里启动grcp server
            if (app.Configuration.UseExtralChannel())
            {
                app.MapGrpcService<GameGrpcService>();
                app.MapGrpcService<SystemGrpcService>();
                app.MapGrpcService<SyncGrpcService>();
                app.MapGrpcService<GuildGrpcService>();
                app.MapGrpcService<AllianceGrpcService>();
                app.MapGrpcService<TeamGrpcService>();
                app.MapGrpcService<ItemGrpcService>();
                app.MapGrpcService<CashGrcpService>();
                app.MapGrpcService<BuddyGrpcService>();
                app.MapGrpcService<DataGrpcService>();
            }
        }
    }
}

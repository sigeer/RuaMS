using Application.Core.Login;
using Application.Core.ServerTransports;
using Application.Module.Duey.Channel;
using Application.Module.Duey.Master;
using Application.Module.ExpeditionBossLog.Master;
using Application.Shared.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Local
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameServerLocal(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();

            // 需要先启动Master
            builder.Services.AddLoginServer(builder.Configuration.GetConnectionString("MySql")!);
            builder.Services.AddChannelServer();

            // 其他可关闭的游戏模块
            builder.Services.AddExpeditionBossLogMaster();
            builder.Services.AddDueyMaster();

            builder.Services.AddDueyChannel();
        }
    }
}

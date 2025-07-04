using Application.Core.Login;
using Application.Core.ServerTransports;
using Application.Module.Duey.Channel;
using Application.Module.Duey.Channel.InProgress;
using Application.Module.ExpeditionBossLog.Master;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Core.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameServerInProgress(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();

            // 需要先启动Master
            builder.Services.AddLoginServer(builder.Configuration.GetConnectionString("MySql")!);
            builder.Services.AddChannelServer();

            // 其他可关闭的游戏模块
            builder.Services.AddExpeditionBossLogMaster();

            builder.Services.AddDueyInProgress();
        }

        public static void UseGameServerLocal(this IHost app)
        {
            app.UseDuey();
        }
    }
}

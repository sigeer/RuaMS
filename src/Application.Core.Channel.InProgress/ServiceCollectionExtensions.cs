using Application.Core.Login;
using Application.Core.ServerTransports;
using Application.Module.Duey.Channel.InProgress;
using Application.Module.ExpeditionBossLog.Master;
using Application.Module.Maker.Channel.InProgress;
using Application.Module.PlayerNPC.Channel.InProgress;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.InProgress
{
    public static class ServiceCollectionExtensions
    {

        public static void AddGameServerInProgress(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();

            // 需要先启动Master
            builder.Services.AddLoginServer(builder.Configuration);
            builder.Services.AddChannelServer();

            // 其他可关闭的游戏模块
            builder.Services.AddExpeditionBossLogMaster();

            builder.Services.AddDueyInProgress();
            builder.Services.AddMakerInProgress();
            builder.Services.AddPlayerNPCInProgress();
        }
    }
}

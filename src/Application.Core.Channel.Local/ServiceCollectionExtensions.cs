using Application.Core.Login;
using Application.Core.ServerTransports;
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
            builder.Services.AddSingleton<ChannelServerConfig>(new ChannelServerConfig());

            // 需要先启动Master
            builder.Services.AddLoginServer(builder.Configuration.GetConnectionString("MySql")!);
            builder.Services.AddChannelServer();
        }
    }
}

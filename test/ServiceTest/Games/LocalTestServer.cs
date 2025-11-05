using Application.Core.Channel;
using Application.Core.Channel.HostExtensions;
using Application.Core.Channel.InProgress;
using Application.Core.Channel.Net;
using Application.Core.Game.Players;
using Application.Core.Login;
using Application.Core.Login.Services;
using Application.Module.Duey.Master;
using Application.Module.ExpeditionBossLog.Master;
using Application.Module.Maker.Master;
using Application.Module.PlayerNPC.Master;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Utility;
using DotNetty.Transport.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ServiceTest.TestUtilities;
using System.Text;
using Yitter.IdGenerator;

namespace ServiceTest.Games
{
    public sealed class LocalTestServer
    {
        public IServiceProvider ServiceProvider { get; }
        public LocalTestServer()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            // 需要先启动Master
            builder.Services.AddLoginServer(builder.Configuration);
            builder.Services.AddDueyMaster();
            builder.Services.AddExpeditionBossLogMaster();
            builder.Services.AddMakerMaster();
            builder.Services.AddPlayerNPCMaster();

            builder.AddChannelServerInProgress();

            builder.Services.AddScoped<IChannel, MockChannel>();

            var debugConfig = new Dictionary<string, string?>
            {
                [$"{AppSettingKeys.Section_WZ}:BaseDir"] = TestVariable.WzPath,
            };
            builder.Configuration.AddInMemoryCollection(debugConfig);

            var app = builder.Build();
            ServiceProvider = app.Services;

            var idGeneratorOptions = new IdGeneratorOptions(1);
            YitIdHelper.SetIdGenerator(idGeneratorOptions);

            app.UseChannelServer();

            var bootstrap = app.Services.GetServices<IServerBootstrap>();
            foreach (var item in bootstrap)
            {
                item.ConfigureHost(app);
            }
        }

        public async Task StartServer()
        {
            var hostedServices = ServiceProvider.GetServices<IHostedService>();
            foreach (var hostService in hostedServices)
            {
                await hostService.StartAsync(CancellationToken.None);
            }
        }

        public MasterServer GetMasterServer() => ServiceProvider.GetRequiredService<MasterServer>();


        public WorldChannel GetChannel(int i)
        {
            var container = ServiceProvider.GetRequiredService<WorldChannelServer>();
            return container.Servers[i];
        }

        public IPlayer? GetPlayer(int cid = 1)
        {
            var channel = ServiceProvider.GetRequiredService<WorldChannelServer>().Servers[1];

            var mainServer = ServiceProvider.GetRequiredService<MasterServer>();
            var loginService = ServiceProvider.GetRequiredService<LoginService>();
            var acc = mainServer.GetAccountDto(1)!;
            acc.CurrentMac = "12345678";
            acc.CurrentHwid = "12345678";
            acc.CurrentIP = "127.0.0.1";
            mainServer.UpdateAccountState(1, LoginStage.LOGIN_SERVER_TRANSITION);
            var obj = loginService.PlayerLogin("12345678", 1);
            var charSrv = ServiceProvider.GetRequiredService<Application.Core.Channel.Services.DataService>();

            var client = ActivatorUtilities.CreateInstance<ChannelClient>(ServiceProvider, (long)1, channel);
            //var mockChannel = new Mock<ChannelClient>();
            //mockChannel.Setup(x => x.getChannel())
            //    .Returns(1);
            return charSrv.Serialize(client, obj);
        }
    }
}

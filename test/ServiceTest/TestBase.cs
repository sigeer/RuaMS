using Application.Core.Channel;
using Application.Core.Channel.Net;
using Application.Core.Game.Players;
using Application.Core.Login;
using Application.Core.Login.Services;
using Application.Core.net.server.coordinator.matchchecker.listener;
using Application.Core.ServerTransports;
using Application.Module.PlayerNPC.Channel.InProgress;
using Application.Shared.Servers;
using Application.Utility.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using net.server.coordinator.matchchecker;
using Serilog;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace ServiceTest
{
    public class TestBase
    {
        protected IServiceProvider _sp;
        public TestBase()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Environment.SetEnvironmentVariable("ms-wz", "D:\\Cosmic\\wz");

            var builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();


            builder.Services.AddSingleton<IChannelServerTransport, Application.Core.Channel.InProgress.LocalChannelServerTransport>();

            // 需要先启动Master
            builder.Services.AddLoginServer(YamlConfig.config.server.DB_CONNECTIONSTRING);
            builder.Services.AddChannelServer();
            builder.Services.AddPlayerNPCInProgress();

            //sc.AddScoped<IChannel, MockChannel>();


            var app = builder.Build();
            _sp = app.Services;

            var idGeneratorOptions = new IdGeneratorOptions(1);
            YitIdHelper.SetIdGenerator(idGeneratorOptions);

            MatchCheckerStaticFactory.Context = new MatchCheckerStaticFactory(
                app.Services.GetRequiredService<MatchCheckerGuildCreationListener>(),
                app.Services.GetRequiredService<MatchCheckerCPQChallengeListener>());

            var bootstrap = app.Services.GetServices<IServerBootstrap>();
            foreach (var item in bootstrap)
            {
                item.ConfigureHost(app);
            }
        }

        protected async Task LoadServer()
        {
            var hostedServices = _sp.GetServices<IHostedService>();
            foreach (var hostService in hostedServices)
            {
                await hostService.StartAsync(CancellationToken.None);
            }
        }

        protected WorldChannel GetChannel(int i)
        {
            var container = _sp.GetRequiredService<WorldChannelServer>();
            return container.Servers[i];
        }

        public IPlayer? GetPlayer()
        {
            var channel = _sp.GetRequiredService<WorldChannelServer>().Servers[1];

            var loginService = _sp.GetRequiredService<LoginService>();
            var obj = loginService.PlayerLogin("", channel.getId(), 1);
            var charSrv = _sp.GetRequiredService<Application.Core.Channel.Services.DataService>();

            var client = ActivatorUtilities.CreateInstance<ChannelClient>(_sp, (long)1, channel);
            //var mockChannel = new Mock<ChannelClient>();
            //mockChannel.Setup(x => x.getChannel())
            //    .Returns(1);
            return charSrv.Serialize(client, obj);
        }

    }
}

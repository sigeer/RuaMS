using Application.Core;
using Application.Core.Channel;
using Application.Core.Channel.Net;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Login;
using Application.Core.Login.Datas;
using Application.Core.Login.ServerTransports;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.EF;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using AutoMapper;
using DotNetty.Transport.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using net.server;
using Serilog;
using Serilog.Events;
using server;
using ServiceTest.TestUtilities;
using System.Text;

namespace ServiceTest
{
    public class TestBase
    {
        protected IServiceProvider _sp;
        public TestBase()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var configuration = new ConfigurationBuilder();
            var config = configuration.Build();

            var sc = new ServiceCollection();
            sc.AddSingleton<IConfiguration>(config);
            sc.AddLogging();

            sc.AddChannelServer();
            sc.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();
            sc.AddSingleton<MultiRunner>();

            sc.AddScoped<IChannel, MockChannel>();

            sc.AddDbFactory(YamlConfig.config.server.DB_CONNECTIONSTRING);
            sc.AddLoginServer();

            _sp = sc.BuildServiceProvider();

            Environment.SetEnvironmentVariable("ms-wz", "D:\\walker\\demo\\MS\\Cosmic\\wz");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GlobalTools.Encoding = Encoding.GetEncoding("GBK");

            TimerManager.InitializeAsync(TaskEngine.Task).Wait();
        }


        public void GetPlayer(Action<IPlayer> action)
        {
            Server.getInstance().LoadWorld();
            var config = new ChannelServerConfig
            {
                Port = 7575
            };
            var scope = _sp.CreateScope();
            var transport = _sp.GetRequiredService<IChannelServerTransport>();
            IWorldChannel channel = new WorldChannel(scope, config, transport);

            channel.GetType().GetField("channel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(channel, 1);
            var charManager = _sp.GetRequiredService<CharacterManager>();
            var obj = charManager.GetCharacter(1);
            var mapper = _sp.GetRequiredService<IMapper>();
            var charSrv = _sp.GetRequiredService<Application.Core.Channel.Services.CharacterService>();

            var client = ActivatorUtilities.CreateInstance<ChannelClient>(_sp, (long)1, channel);
            //var mockChannel = new Mock<ChannelClient>();
            //mockChannel.Setup(x => x.getChannel())
            //    .Returns(1);
            action(charSrv.Serialize(client, obj));
        }

    }
}

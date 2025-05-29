using Application.Core;
using Application.Core.Channel;
using Application.Core.Channel.Local;
using Application.Core.Channel.Net;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Login;
using Application.Core.Login.Datas;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using AutoMapper;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using net.server;
using Serilog;
using Serilog.Events;
using server;
using ServiceTest.TestUtilities;
using System.Text;
using System.Threading.Channels;
using Yitter.IdGenerator;

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

            var idGeneratorOptions = new IdGeneratorOptions(1);
            YitIdHelper.SetIdGenerator(idGeneratorOptions);

            // Environment.SetEnvironmentVariable("ms-wz", "D:\\Cosmic\\wz");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GlobalTools.Encoding = Encoding.GetEncoding("GBK");

            TimerManager.InitializeAsync(TaskEngine.Task).Wait();
        }

        public WorldChannel CreateChannel()
        {
            Server.getInstance().LoadWorld();
            var config = new ChannelServerConfig
            {
                Port = 7575
            };
            var scope = _sp.CreateScope();
            var transport = _sp.GetRequiredService<IChannelServerTransport>();
            return new WorldChannel(scope, config, transport);
        }

        public IPlayer? GetPlayer()
        {
            var channel = CreateChannel();

            channel.GetType().GetField("channel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(channel, 1);
            var masterServer = _sp.GetRequiredService<MasterServer>();
            var obj = masterServer.CharacterManager.GetCharacter(1);
            var charSrv = _sp.GetRequiredService<Application.Core.Servers.Services.CharacterService>();

            var client = ActivatorUtilities.CreateInstance<ChannelClient>(_sp, (long)1, channel);
            //var mockChannel = new Mock<ChannelClient>();
            //mockChannel.Setup(x => x.getChannel())
            //    .Returns(1);
            return charSrv.Serialize(client, obj);
        }

    }
}

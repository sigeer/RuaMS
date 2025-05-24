using Application.Core;
using Application.Core.Game;
using Application.Core.Game.Players;
using Application.EF;
using Application.Utility.Tasks;
using constants.id;
using net.server;
using Serilog.Events;
using Serilog;
using server;
using System.Text;
using Application.Core.Client;
using Microsoft.Extensions.DependencyInjection;
using Application.Core.Login;
using Application.Core.Channel;
using Application.Core.Login.ServerTransports;
using Application.Core.ServerTransports;
using Microsoft.EntityFrameworkCore;
using Application.Utility.Configs;
using Application.Core.Servers;
using DotNetty.Transport.Channels.Local;
using Application.Core.Channel.Net;
using Application.Core.Login.Services;
using tools;
using Application.Core.Login.Datas;
using DotNetty.Transport.Channels;
using ServiceTest.TestUtilities;
using Moq;
using Microsoft.Extensions.Configuration;
using Application.Core.Game.TheWorld;
using AutoMapper;
using Application.Shared.Characters;

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
            sc.AddDbContextFactory<DBContext>(o =>
            {
                o.UseMySQL(YamlConfig.config.server.DB_CONNECTIONSTRING);
            });
            sc.AddChannelServer();
            sc.AddSingleton<IChannelServerTransport, LocalChannelServerTransport>();
            sc.AddSingleton<MultiRunner>();

            sc.AddScoped<IChannel, MockChannel>();

            sc.AddLoginServer();

            _sp = sc.BuildServiceProvider();

            Environment.SetEnvironmentVariable("ms-wz", "C:\\Demo\\MS\\wz");

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
            var accManager = _sp.GetRequiredService<AccountManager>();
            var mapper = _sp.GetRequiredService<IMapper>();
            obj.Account = mapper.Map<AccountDto>(accManager.GetAccountEntity(obj.Character.AccountId));
            var charSrv = _sp.GetRequiredService<Application.Core.Channel.Services.CharacterService>();

            var client = ActivatorUtilities.CreateInstance<ChannelClient>(_sp, (long)1, channel);
            //var mockChannel = new Mock<ChannelClient>();
            //mockChannel.Setup(x => x.getChannel())
            //    .Returns(1);
            action(charSrv.Serialize(client, obj));
        }

    }
}

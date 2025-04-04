using Application.Core;
using Application.Core.Game;
using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.EF;
using Application.Utility.Tasks;
using constants.id;
using net.server;
using Serilog.Events;
using Serilog;
using server;
using System.Text;

namespace ServiceTest
{
    public class TestBase
    {
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
            Environment.SetEnvironmentVariable("ms-wz", "C:\\Demo\\MS\\wz");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GlobalTools.Encoding = Encoding.GetEncoding("GBK");

            TimerManager.InitializeAsync(TaskEngine.Task).Wait();

            LoadTestWorld();
        }



        private void LoadTestWorld()
        {
            using var dbContext = new DBContext();
            var testWorldConfig = dbContext.WorldConfigs.FirstOrDefault();
            if (testWorldConfig != null)
                Server.getInstance().InitWorld(testWorldConfig).Wait();
        }

        private IClient? _client;
        protected IClient MockClient => _client ??= GetOnlinedTestClient();

        protected IClient GetOnlinedTestClient(int charId = 1)
        {
            Server.getInstance().forceUpdateCurrentTime();
            var mockClient = Client.createMock();
            mockClient.World = 0;
            mockClient.Channel = 1;
            GetMockPlayer(mockClient, charId);
            return mockClient;
        }

        private IPlayer GetMockPlayer(IClient client, int charId = 1)
        {
            var player = CharacterManager.LoadPlayerFromDB(charId, client, true)!;
            client.setPlayer(player);
            player.setClient(client);
            player.setMap(Server.getInstance().getChannel(0, 1).getMapFactory().getMap(MapId.HENESYS));
            player.setEnteredChannelWorld(1);
            return player;
        }
    }
}

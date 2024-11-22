using Application.Core;
using Application.Core.Compatible;
using Application.Core.Game;
using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.EF;
using constants.id;
using net.server;
using Quartz.Impl;
using server;
using server.maps;
using System.Text;

namespace ServiceTest
{
    public class TestBase
    {
        public TestBase()
        {
            Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GlobalTools.Encoding = Encoding.GetEncoding("GBK");

            TimerManager.Initialize().Wait();

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
            var mockClient = new MockupClient();
            GetMockPlayer(mockClient, charId);
            return mockClient;
        }

        private IPlayer GetMockPlayer(IClient client, int charId = 1)
        {
            var player = CharacterManager.GetPlayerById(charId)!;
            client.setPlayer(player);
            player.setClient(client);
            player.setMap(Server.getInstance().getChannel(0, 1).getMapFactory().getMap(MapId.HENESYS));
            player.setEnteredChannelWorld(1);
            return player;
        }
    }
}

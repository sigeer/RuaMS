using Application.Core;
using Application.Core.Compatible;
using Application.Core.Game;
using Application.Core.Game.Players;
using Application.Core.Managers;
using constants.id;
using net.server;
using Quartz.Impl;
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

            var factory = new StdSchedulerFactory();
            SchedulerManage.Scheduler = factory.GetScheduler().Result;

            Server.getInstance().addWorld();
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
            player.setMap(new MapManager(null, 0, 1).getMap(MapId.HENESYS));
            player.setEnteredChannelWorld(1);
            return player;
        }
    }
}

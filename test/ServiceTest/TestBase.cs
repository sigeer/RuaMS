using Application.Core.Compatible;
using Application.Core.Game;
using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.Core.scripting.Event.jobs;
using constants.id;
using net.server;
using Quartz.Impl;
using server.maps;

namespace ServiceTest
{
    public class TestBase
    {
        public TestBase()
        {
            Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");
        }
        protected IClient GetOnlinedTestClient(int charId = 1)
        {
            var factory = new StdSchedulerFactory();
            SchedulerManage.Scheduler = factory.GetScheduler().Result;
            SchedulerManage.Scheduler.ListenerManager.AddJobListener(new JobCompleteListener());

            Server.getInstance().forceUpdateCurrentTime();
            var mockClient = new MockupClient();
            GetMockPlayer(mockClient, charId);
            return mockClient;
        }

        protected IPlayer GetMockPlayer(IClient client, int charId = 1)
        {
            var player = CharacterManager.GetPlayerById(charId)!;
            client.setPlayer(player);
            player.setClient(client);
            player.setMap(new MapManager(null, 0, 1).getMap(MapId.HENESYS));
            return player;
        }
    }
}

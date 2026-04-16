namespace ServiceTest.Games.Gameplay
{
    internal class NpcTalkTests
    {
        [Test]
        public async Task Npc1012112Test()
        {
            var chr = GameTestGlobal.TestServer.GetPlayer();
            var map = chr.getChannelServer().getMapFactory().getMap(100000200);
            chr.changeMap(map);
            chr.setMapTransitionComplete();

            map.Send((mapChr) =>
            {
                Assert.That(chr.MapModel.Id is 100000200);
                Assert.That(chr.Client.CurrentServer.NPCScriptManager.start(chr.Client, 1012112, null));
                // 选项 我想兑换一件年糕的帽子。 
                Assert.That(chr.Client.CurrentServer.NPCScriptManager.action(chr.Client, 1, 1, 2));
            });
            await Task.Delay(500000);

        }
    }
}

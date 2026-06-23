namespace ServiceTest.Games.Gameplay
{
    internal class NpcTalkTests
    {
        [Test]
        public async Task Npc1012112Test()
        {
            var chr = await GameTestGlobal.TestServer.GetPlayer();
            var map = await chr.getChannelServer().getMapFactory().getMap(100000200);
            var oldMap = chr.MapModel;
            await chr.changeMap(map);
            chr.setMapTransitionComplete();

            await Task.Delay(5000);

            Assert.That(chr.MapModel.Id is 100000200);
            Assert.That(await chr.Client.CurrentServer.NPCScriptManager.start(chr.Client, 1012112, null));
            // 选项 我想兑换一件年糕的帽子。 
            Assert.That(await chr.Client.CurrentServer.NPCScriptManager.action(chr.Client, 1, 1, 2));
        }
    }
}

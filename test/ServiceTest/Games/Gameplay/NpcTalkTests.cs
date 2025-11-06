namespace ServiceTest.Games.Gameplay
{
    internal class NpcTalkTests
    {
        [Test]
        public void Npc1012112Test()
        {
            var chr = GameTestGlobal.TestServer.GetPlayer();
            chr.changeMap(100000200);
            Assert.That(chr.Client.CurrentServer.NPCScriptManager.start(chr.Client, 1012112, null));
            // 选项 我想兑换一件年糕的帽子。 
            Assert.That(chr.Client.CurrentServer.NPCScriptManager.action(chr.Client, 1, 1, 2));
        }
    }
}

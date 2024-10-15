using scripting.npc;

namespace ServiceTest.Scripts
{
    public class NPCScriptTests : TestBase
    {
        [Test]
        public void Script_2100_Test()
        {
            Assert.That(NPCScriptManager.getInstance().start(MockClient, 2100, null));
        }

        [Test]
        public void Script_CommandJs_Test()
        {
            var client = MockClient;
            Assert.That(NPCScriptManager.getInstance().start(client, 0, "commands", null));
            NPCScriptManager.getInstance().action(client, 1, 1, 2);
            Assert.Pass();
        }
    }
}

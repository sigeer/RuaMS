using Application.Core.Game;
using constants.id;
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

        [Test]
        public void Script_2012006_Test()
        {
            MockClient.OnlinedCharacter.changeMap(MapId.MUSHROOM_TOWN);
            Assert.That(MockClient.OnlinedCharacter.MapModel.getId() == MapId.MUSHROOM_TOWN);
            NPCScriptManager.getInstance().start(MockClient, 2012006, null);
            NPCScriptManager.getInstance().action(MockClient, 1, 1, 2);
            NPCScriptManager.getInstance().action(MockClient, 1, 1, 1);
            Assert.That(MockClient.OnlinedCharacter.MapModel.getId() == 200000130);
        }
    }
}

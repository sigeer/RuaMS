using Application.Core.Game;

namespace ServiceTest.Clients
{
    public class ServerClientTests
    {
        public ServerClientTests()
        {
            Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");
        }
        [Test]
        public void LoadCharacterListTest()
        {
            var testClient = Client.createMock();
            testClient.setAccID(1);
            var players = testClient.loadCharacters(0);
            Assert.That(players.Count, Is.EqualTo(1));
            if (players.Count > 0)
                Assert.That(players[0].Bag[client.inventory.InventoryType.EQUIPPED].Count() > 0);
        }
    }
}

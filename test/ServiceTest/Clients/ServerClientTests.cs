using Application.Core.Game;
using Application.EF;
using Microsoft.EntityFrameworkCore;
using net.server.coordinator.session;
using System.Diagnostics;

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

        [Test]
        public void LoginTest()
        {
            var testClient = Client.createMock();
            using var dbContext = new DBContext();
            dbContext.Accounts.ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, 0));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var loginResult = testClient.login("admin", "admin", new Hwid("123456"));
            Assert.That(loginResult, Is.EqualTo(0));
            var state = testClient.finishLogin();
            Assert.That(state, Is.EqualTo(0));
            sw.Stop();
            Console.WriteLine($"Login will cost {sw.Elapsed.TotalSeconds}");
        }
    }
}

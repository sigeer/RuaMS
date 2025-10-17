using Application.Core.Login.Client;
using Application.Shared.Login;
using Application.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using ServiceTest.Games;

namespace ServiceTest.Games.Account
{
    public class ServerClientTests
    {
        //[Test]
        //public void LoadCharacterListTest()
        //{
        //    var testClient = Client.createMock();
        //    testClient.setAccID(1);
        //    var players = testClient.loadCharacters(0);
        //    Assert.That(players.Count > 0);
        //    if (players.Count > 0)
        //        Assert.That(players[0].Bag[client.inventory.InventoryType.EQUIPPED].Count() > 0);
        //}

        //[Test]
        //public void LoginTest()
        //{
        //    var testClient = ActivatorUtilities.CreateInstance<LoginClient>(GlobalSetup.TestServer.ServiceProvider, (long)1, GlobalSetup.TestServer.GetMasterServer());
        //    var loginResult = testClient.Login("admin", "admin", new Hwid("123456"));
        //    Assert.That(loginResult, Is.EqualTo(LoginResultCode.Success));
        //    var state = testClient.FinishLogin();
        //    Assert.That(state, Is.EqualTo(LoginResultCode.Success));
        //}
    }
}

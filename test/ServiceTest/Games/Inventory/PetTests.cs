using Application.Core.Game.Items;
using Application.Core.Login;
using Application.Core.Login.Datas;
using Application.Core.Login.ServerData;
using Application.Core.Login.Services;
using Application.Shared.Constants.Inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.DependencyInjection;
using ServiceTest.Games;

namespace ServiceTest.Games.Inventory
{
    public class PetTests
    {
        [Test]
        public async Task LoadPetDataTest()
        {
            var chr = GlobalSetup.TestServer.GetPlayer();
            var server = GlobalSetup.TestServer.ServiceProvider.GetRequiredService<MasterServer>();
            var inv = chr.Bag[InventoryType.CASH];
            var old = inv.list();
            Assert.That(InventoryManipulator.addById(chr.Client, 5000041, 1));
            Assert.That(inv.list().Count == old.Count + 1);

            var item = inv.findById(5000041);
            Assert.That(item is Pet);

            chr.saveCharToDB(Application.Shared.Events.SyncCharacterTrigger.ChangeServer);
            var storageSerivce = GlobalSetup.TestServer.ServiceProvider.GetRequiredService<ServerManager>();
            await storageSerivce.CommitAllImmediately();
            //模拟重新登录
            var chr1 = GlobalSetup.TestServer.GetPlayer()!;
            var inv1 = chr1.Bag[InventoryType.CASH];
            var item1 = inv1.findById(5000041);
            Assert.That(item1 is Pet);

            server.CharacterManager.Dispose();
            // 模拟第一次登录
            var chr2 = GlobalSetup.TestServer.GetPlayer();
            var inv2 = chr2.Bag[InventoryType.CASH];
            var item2 = inv2.findById(5000041);
            Assert.That(item2 is Pet);

            InventoryManipulator.removeById(chr.Client, InventoryType.CASH, 5000041, 1, false, false);
            chr.saveCharToDB(Application.Shared.Events.SyncCharacterTrigger.ChangeServer);
            await storageSerivce.CommitAllImmediately();
        }
    }
}

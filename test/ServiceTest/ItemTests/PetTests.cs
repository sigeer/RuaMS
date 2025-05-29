using Application.Core.Game.Items;
using Application.Core.Login;
using Application.Core.Login.Services;
using Application.Shared.Constants.Inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceTest.ItemTests
{
    public class PetTests : TestBase
    {
        [Test]
        public async Task LoadPetDataTest()
        {
            var chr = GetPlayer();
            var server = _sp.GetRequiredService<MasterServer>();
            var inv = chr.Bag[InventoryType.CASH];
            var old = inv.list();
            Assert.That(InventoryManipulator.addById(chr.Client, 5000041, 1));
            Assert.That(inv.list().Count == old.Count + 1);

            var item = inv.findById(5000041);
            Assert.That(item is Pet);

            chr.saveCharToDB();
            var storageSerivce = _sp.GetRequiredService<StorageService>();
            await storageSerivce.CommitAllImmediately();
            //模拟重新登录
            var chr1 = GetPlayer()!;
            var inv1 = chr1.Bag[InventoryType.CASH];
            var item1 = inv1.findById(5000041);
            Assert.That(item1 is Pet);

            server.CharacterManager.Dispose();
            // 模拟第一次登录
            var chr2 = GetPlayer();
            var inv2 = chr2.Bag[InventoryType.CASH];
            var item2 = inv2.findById(5000041);
            Assert.That(item2 is Pet);

            InventoryManipulator.removeById(chr.Client, InventoryType.CASH, 5000041, 1, false, false);
            chr.saveCharToDB();
            await storageSerivce.CommitAllImmediately();
        }
    }
}

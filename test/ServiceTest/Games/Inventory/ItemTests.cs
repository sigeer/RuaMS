using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.EF;
using Application.Shared.Constants.Inventory;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ServiceTest.Games.Inventory
{
    internal class ItemTests
    {
        [Test]
        public void MonsterCardTest()
        {
            var newProvider = ProviderSource.Instance.GetProvider<ItemProvider>();

            var dict = newProvider.GetAllMonsterCard().OrderBy(x => x.TemplateId).ToDictionary(x => x.TemplateId, x => x.MobId);
            var str1 = JsonConvert.SerializeObject(dict);

            var options = new DbContextOptionsBuilder<DBContext>()
                    .UseSqlite("Data Source=database.db")
                    .Options;

            using var dbContext = new DBContext(options);
            var dataFromDb = dbContext.Monstercarddata.OrderBy(x => x.Cardid).ToList().ToDictionary(x => x.Cardid, x => x.Mobid);
            var str2 = JsonConvert.SerializeObject(dataFromDb);
            // db的数据似乎有大量不准确的，可以放弃使用这张表
            Console.WriteLine(str1);
            Console.WriteLine(str2);
            Assert.That(dict.Count, Is.EqualTo(dataFromDb.Count));
        }

        [Test]
        public void InventorySortTest()
        {
            var chr = GameTestGlobal.TestServer.GetPlayer();

            var invType = InventoryType.USE;
            // 加入测试道具
            chr.GainItem(2000007, 100);
            chr.GainItem(2000004, (short)(ItemInformationProvider.getInstance().getSlotMax(chr.Client, 2000004) + 1));
            chr.GainItem(2000005, 100);
            chr.GainItem(2000006, 100);

            var chrInv = chr.GetInventory(invType);
            var sorter = new BagInventorySorter(chrInv);
            sorter.Sort();

            var p1 = chrInv.LoadAllSlot();
            for (int i = 0; i < p1.Count; i++)
            {
                if (p1[i] != null)
                {
                    Assert.That(p1[i]!.getPosition() - 1 == i);
                }
            }

            sorter.Move(1, 10);
            sorter.Sort();

            var p2 = chrInv.LoadAllSlot();

            Assert.That(p2, Is.EqualTo(p1));
        }
    }
}

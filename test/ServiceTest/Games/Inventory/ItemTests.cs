using Application.EF;
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
            var newProvider = ProviderFactory.GetProvider<ItemProvider>();

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
    }
}

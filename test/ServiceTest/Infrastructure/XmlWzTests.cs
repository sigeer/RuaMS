using Application.Templates.Item.Consume;
using Application.Templates.Item.Data;
using Application.Templates.XmlWzReader;
using Application.Templates.XmlWzReader.Provider;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using XmlWzReader;

namespace ServiceTest.Infrastructure
{
    internal class XmlWzTests
    {
        int mapId = 100000000;
        private static string GetMapImg(int mapid)
        {
            string mapName = mapid.ToString().PadLeft(9, '0');
            StringBuilder builder = new StringBuilder("Map/Map");
            int area = mapid / 100000000;
            builder.Append(area);
            builder.Append("/");
            builder.Append(mapName);
            builder.Append(".img");
            mapName = builder.ToString();
            return mapName;
        }

        [Test]
        public void XMLDomMapleData_Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var provider = DataProviderFactory.getDataProvider(XmlWzReader.wz.WZFiles.MAP);
            var fullData = provider.getData(GetMapImg(mapId));
            sw.Stop();
            Console.WriteLine(sw);
            Assert.That(DataTool.GetBoolean("info/town", fullData));
        }

        [Test]
        public void NewProvider_Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var provider = new MapProvider(new Application.Templates.TemplateOptions());
            var data = provider.GetItem(mapId);
            sw.Stop();
            Console.WriteLine(sw);
            Assert.That(data?.Town ?? false);
        }

        [Test]
        public void ConsumeItemTemplateMapCheck()
        {
            var provider = new ItemProvider(new Application.Templates.TemplateOptions());
            var townScrollItem = provider.GetRequiredItem<TownScrollItemTemplate>(2031000);
            Assert.That(townScrollItem!.MoveTo, Is.EqualTo(229010000));
            Assert.That(townScrollItem!.IgnoreContinent, Is.EqualTo(true));

            var petFoodItem = provider.GetRequiredItem<PetFoodItemTemplate>(2120000)!;
            Assert.That(petFoodItem.PetfoodInc, Is.EqualTo(30));
            Assert.That(petFoodItem.Pet, Does.Contain(5000005));

            var scrollItem = provider.GetRequiredItem<ScrollItemTemplate>(2040017)!;
            Assert.That(scrollItem.SuccessRate, Is.EqualTo(60));
            Assert.That(scrollItem.IncDEX, Is.EqualTo(1));
            Assert.That(scrollItem.IncACC, Is.EqualTo(2));

            var solomenItem = provider.GetRequiredItem<SolomenItemTemplate>(2370009)!;
            Assert.That(solomenItem.MaxLevel, Is.EqualTo(50));
            Assert.That(solomenItem.Exp, Is.EqualTo(500));

            var skill228 = provider.GetRequiredItem<MasteryItemTemplate>(2280014)!;
            Assert.That(skill228.MasterLevel, Is.EqualTo(10));
            Assert.That(skill228.Skills, Does.Contain(21120004));

            var skill229 = provider.GetRequiredItem<MasteryItemTemplate>(2290086)!;
            Assert.That(skill229.MasterLevel, Is.EqualTo(20));
            Assert.That(skill229.ReqSkillLevel, Is.EqualTo(5));
            Assert.That(skill229.SuccessRate, Is.EqualTo(70));
            Assert.That(skill229.Skills, Does.Contain(4121008));

            var catchItem = provider.GetRequiredItem<CatchMobItemTemplate>(2270005)!;
            Assert.That(catchItem.MobHP, Is.EqualTo(30));
            Assert.That(catchItem.Mob, Is.EqualTo(9300187));

            var bulletItem = provider.GetRequiredItem<BulletItemTemplate>(2070011)!;
            Assert.That(bulletItem.ReqLevel, Is.EqualTo(10));
            Assert.That(bulletItem.IncPAD, Is.EqualTo(21));

            var summonItem = provider.GetRequiredItem<SummonMobItemTemplate>(2101054)!;
            Assert.That(summonItem.SummonData, Has.Some.Matches<SummonData>(p => p.Mob == 4230119 && p.Prob == 100));

            var monsterCardItem = provider.GetRequiredItem<MonsterCardItemTemplate>(2384030)!;
            Assert.That(monsterCardItem.MobId, Is.EqualTo(7130000));
            Assert.That(monsterCardItem.Time, Is.EqualTo(1200000));
            Assert.That(monsterCardItem.Prob, Is.EqualTo(15));
            Assert.That(monsterCardItem.Con, Has.Some.Matches<ConData>(p => p.StartMap == 200010100 && p.EndMap == 200010999));

            var potionItem_2022421 = provider.GetRequiredItem<PotionItemTemplate>(2022421)!;
            Assert.That(potionItem_2022421.MMPR, Is.EqualTo(90));

            var potionItem_2022422 = provider.GetRequiredItem<PotionItemTemplate>(2022422)!;
            Assert.That(potionItem_2022422.Reward, Has.Some.Matches<RewardData>(p => p.ItemID == 4001201 && p.Prob == 17));

            var potionItem_2050004 = provider.GetRequiredItem<PotionItemTemplate>(2050004)!;
            Assert.That(potionItem_2050004.Cure_Curse, Is.EqualTo(true));
        }
    }
}

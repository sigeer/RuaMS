using Application.Templates.Character;
using Application.Templates.Item;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using ServiceTest.TestUtilities;
using System.Drawing;

namespace ServiceTest.Infrastructure.WZ
{
    internal class ItemTests
    {
        public ItemTests()
        {
            ProviderFactory.Clear();
            ProviderFactory.Configure(o =>
            {
                o.DataDir = TestVariable.WzPath;

                o.RegisterProvider<ItemProvider>(() => new ItemProvider(new Application.Templates.TemplateOptions()));
                o.RegisterProvider<EquipProvider>(() => new EquipProvider(new Application.Templates.TemplateOptions()));
            });
        }

        [Test]
        public void CashItemTemplateDataCheck()
        {
            var provider = ProviderFactory.GetProvider<ItemProvider>();

            var areaEffectItem = provider.GetRequiredItem<AreaEffectItemTemplate>(5281000)!;
            Assert.That(areaEffectItem.Time, Is.EqualTo(60));
            Assert.That(areaEffectItem.LT, Is.EqualTo(new Point(-110, -82)));
            Assert.That(areaEffectItem.RB, Is.EqualTo(new Point(110, 83)));

            var petFoodItem = provider.GetRequiredItem<CashPetFoodItemTemplate>(5240005)!;
            Assert.That(petFoodItem.PetfoodInc, Is.EqualTo(100));
            Assert.That(petFoodItem.Pet, Does.Contain(5000007));

            var couponItem = provider.GetRequiredItem<CouponItemTemplate>(5211004)!;
            Assert.That(couponItem.Rate, Is.EqualTo(2));
            Assert.That(couponItem.Time, Is.EqualTo(int.MaxValue));
            Assert.That(couponItem.TimeRange, Does.Contain("TUE:07-11"));

            var extendItem = provider.GetRequiredItem<ExtendItemTimeItemTemplate>(05500002)!;
            Assert.That(extendItem.AddTime, Is.EqualTo(1728000));
            Assert.That(extendItem.MaxDays, Is.EqualTo(30));


            var hiredItem = provider.GetRequiredItem<HiredMerchantItemTemplate>(05030008)!;
            Assert.That(hiredItem.NotifyWhenSold, Is.EqualTo(true));

            var mapBuffItem = provider.GetRequiredItem<MapBuffItemTemplate>(05121009)!;
            Assert.That(mapBuffItem.StateChangeItem, Is.EqualTo(2022154));

            var mesoItem = provider.GetRequiredItem<MesoBagItemTemplate>(05200001)!;
            Assert.That(mesoItem.Meso, Is.EqualTo(5000000));

            var morphItem = provider.GetRequiredItem<MorphItemTemplate>(05300002)!;
            Assert.That(morphItem.Morph, Is.EqualTo(3));
            Assert.That(morphItem.Time, Is.EqualTo(600000));
            Assert.That(morphItem.HP, Is.EqualTo(50));

            var safetyCharmItem = provider.GetRequiredItem<SafetyCharmItemTemplate>(05130000)!;
            Assert.That(safetyCharmItem.RecoveryRate, Is.EqualTo(30));
        }

        [Test]
        public void ConsumeItemTemplateDataCheck()
        {
            var provider = ProviderFactory.GetProvider<ItemProvider>();
            var townScrollItem = provider.GetRequiredItem<TownScrollItemTemplate>(2031000);
            Assert.That(townScrollItem!.MoveTo, Is.EqualTo(229010000));
            Assert.That(townScrollItem!.IgnoreContinent, Is.EqualTo(true));

            var petFoodItem = provider.GetRequiredItem<PetFoodItemTemplate>(2120000)!;
            Assert.That(petFoodItem.PetfoodInc, Is.EqualTo(30));
            Assert.That(petFoodItem.Pet, Does.Contain(5000005));

            var scrollItem = provider.GetRequiredItem<ScrollItemTemplate>(02040111)!;
            Assert.That(scrollItem.Success, Is.EqualTo(60));
            Assert.That(scrollItem.IncDEX, Is.EqualTo(1));
            Assert.That(scrollItem.Req, Does.Contain(1012017));

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

        [Test]
        public void EtcItemTemplateDataCheck()
        {
            var provider = ProviderFactory.GetProvider<ItemProvider>();

            var item = provider.GetRequiredItem<EtcItemTemplate>(4000113)!;
            Assert.That(item.lv, Is.EqualTo(34));

            var incubatorItem = provider.GetRequiredItem<IncubatorItemTemplate>(4220129)!;
            Assert.That(incubatorItem.Grade, Is.EqualTo(3));
            Assert.That(incubatorItem.QuestID, Is.EqualTo(8252));
            Assert.That(incubatorItem.ConsumeItems, Has.Some.Matches<IncubatorConsumeItem>(p => p.ItemId == 4032135 && p.Value == 1));
        }


        [Test]
        public void EquipItemTemplateDataCheck()
        {
            var provider = ProviderFactory.GetProvider<EquipProvider>();
            var item = provider.GetRequiredItem<EquipTemplate>(01002430)!;

            Console.WriteLine(JsonConvert.SerializeObject(item));
            Assert.That(item.ReqLevel, Is.EqualTo(60));

        }
    }
}

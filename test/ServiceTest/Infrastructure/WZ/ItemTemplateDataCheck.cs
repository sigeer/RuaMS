using Application.Templates;
using Application.Templates.Item;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Install;
using Application.Templates.Item.Pet;
using Application.Templates.Reader;
using System.Drawing;

namespace ServiceTest.Infrastructure.WZ;

internal class ItemTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.Item);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var all = provider.LoadAll().OfType<AbstractItemTemplate>().ToList();
        Assert.That(all.All(i => i.TemplateId > 0), Is.True);
    }

    [Test]
    public void NonExistentItem_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetItem(-1);
        Assert.That(item, Is.Null);
    }

    [Test]
    public void GetItem_ConsumeItem_ReturnsCorrectTemplate()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetItem(2000001);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.TemplateId, Is.EqualTo(2000001));
        Assert.That(item.Cash, Is.False);
    }

    [Test]
    public void GetItem_InstallItem_ReturnsCorrectTemplate()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetItem(3010000);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.TemplateId, Is.EqualTo(3010000));
    }

    [Test]
    public void GetItem_EtcItem_ReturnsCorrectTemplate()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetItem(4000000);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.TemplateId, Is.EqualTo(4000000));
    }

    [Test]
    public void GetItem_CashItem_ReturnsCorrectTemplate()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetItem(5281000);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.TemplateId, Is.EqualTo(5281000));
        Assert.That(item.Cash, Is.True);
    }

    [Test]
    public void AbstractItemTemplateBaseDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var item = provider.GetRequiredItem<AbstractItemTemplate>(2240000)!;
        Assert.That(item.AccountSharable, Is.False);
        Assert.That(item.TradeAvailable, Is.False);
        Assert.That(item.SlotMax, Is.EqualTo(1));
        Assert.That(item.Cash, Is.False);
        Assert.That(item.TradeBlock, Is.True);
        Assert.That(item.NotSale, Is.True);
        Assert.That(item.Quest, Is.False);
        Assert.That(item.Only, Is.True);
        Assert.That(item.Price, Is.EqualTo(1));
        Assert.That(item.Time, Is.EqualTo(-1));
        Assert.That(item.TimeLimited, Is.False);
        Assert.That(item.ExpireOnLogout, Is.False);
    }

    [Test]
    public void ItemTemplateBaseDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var item = provider.GetRequiredItem<ItemTemplateBase>(2240000)!;
        Assert.That(item.MCType, Is.EqualTo(0));
        Assert.That(item.PartyQuest, Is.False);
        Assert.That(item.Max, Is.EqualTo(0));

        var item244 = provider.GetRequiredItem<ItemTemplateBase>(2440000)!;
        Assert.That(item244.PartyQuest, Is.True);
    }

    [Test]
    public void ConsumeItemTemplateBaseDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetRequiredItem<ConsumeItemTemplate>(2320000)!;

        Assert.That(item.MonsterBook, Is.False);
        Assert.That(item.ConsumeOnPickup, Is.False);
        Assert.That(item.Party, Is.False);
        Assert.That(item.NoCancelMouse, Is.False);
        Assert.That(item.InfoType, Is.EqualTo(0));
        Assert.That(item.TimeLimited, Is.True);
        Assert.That(item.SlotMax, Is.EqualTo(1));
        Assert.That(item.Only, Is.True);
        Assert.That(item.Price, Is.EqualTo(1));
        Assert.That(item.TradeBlock, Is.True);
        Assert.That(item.NotSale, Is.True);

        var item244 = provider.GetRequiredItem<ConsumeItemTemplate>(2440000)!;
        Assert.That(item244.PartyQuest, Is.True);
    }

    [Test]
    public void ConsumeItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
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
        Assert.That(catchItem.UseDelay, Is.EqualTo(0));
        Assert.That(catchItem.Create, Is.EqualTo(2109001));
        Assert.That(catchItem.DelayMsg, Is.Null);
        Assert.That(catchItem.BridleProp, Is.EqualTo(0));

        var bulletItem = provider.GetRequiredItem<BulletItemTemplate>(2070011)!;
        Assert.That(bulletItem.ReqLevel, Is.EqualTo(10));
        Assert.That(bulletItem.IncPAD, Is.EqualTo(21));
        Assert.That(bulletItem.UnitPrice, Is.EqualTo(1.0));

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
    public void OtherConsumeItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var fatigueItem = provider.GetRequiredItem<OtherConsumeItemTemplate>(2260000)!;
        Assert.That(fatigueItem.IncFatigue, Is.EqualTo(-30));
        Assert.That(fatigueItem.Price, Is.EqualTo(500));
        Assert.That(fatigueItem.SlotMax, Is.EqualTo(200));

        var expBuffItem = provider.GetRequiredItem<OtherConsumeItemTemplate>(2450000)!;
        Assert.That(expBuffItem.ExpBuffRate, Is.EqualTo(200));
        Assert.That(expBuffItem.Time, Is.EqualTo(1800000));
        Assert.That(expBuffItem.TradeBlock, Is.True);
        Assert.That(expBuffItem.NotSale, Is.True);
        Assert.That(expBuffItem.TimeLimited, Is.True);
        Assert.That(expBuffItem.SlotMax, Is.EqualTo(1));
    }

    [Test]
    public void ScriptItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var scriptItem = provider.GetRequiredItem<ScriptItemTemplate>(2430000)!;
        Assert.That(scriptItem.Script, Is.EqualTo("firstSignus"));
        Assert.That(scriptItem.Npc, Is.EqualTo(9000046));
        Assert.That(scriptItem.RunOnPickup, Is.False);
        Assert.That(scriptItem.TradeBlock, Is.True);
        Assert.That(scriptItem.SlotMax, Is.EqualTo(20));
        Assert.That(scriptItem.NotSale, Is.True);
        Assert.That(scriptItem.Quest, Is.True);

        var pickupItem = provider.GetRequiredItem<ScriptItemTemplate>(2430010)!;
        Assert.That(pickupItem.RunOnPickup, Is.True);
        Assert.That(pickupItem.Script, Is.EqualTo("openTreasure"));
        Assert.That(pickupItem.Npc, Is.EqualTo(2040030));
    }

    [Test]
    public void GhostItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetRequiredItem<GhostItemTemplate>(2360000)!;

        Assert.That(item.Ghost, Is.EqualTo(1));
        Assert.That(item.Time, Is.EqualTo(3600000));
        Assert.That(item.Price, Is.EqualTo(1));
        Assert.That(item.SlotMax, Is.EqualTo(200));
    }

    [Test]
    public void CashItemTemplateBaseDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetRequiredItem<CashItemTemplate>(5010000)!;

        Assert.That(item.ProtectTime, Is.EqualTo(0));
        Assert.That(item.Cash, Is.True);
    }

    [Test]
    public void CashItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

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
        Assert.That(couponItem.TimeRangeF, Has.Some.Matches<WeeklyTimeRange>(x =>
                    x.Contains(new DateTime(2026, 6, 2, 7, 0, 0))
                    && x.Contains(new DateTime(2026, 6, 2, 8, 0, 0))
                    && !x.Contains(new DateTime(2026, 6, 2, 11, 0, 1))));

        var couponItem2 = provider.GetRequiredItem<CouponItemTemplate>(05211046)!;
        Assert.That(couponItem2.Rate, Is.EqualTo(2));
        Assert.That(couponItem2.TimeRangeF, Has.Some.Matches<WeeklyTimeRange>(x =>
                    x.Contains(DateTime.Now)));

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
    public void CashPackagedItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetRequiredItem<CashPackagedItemTemplate>(5530000)!;

        Assert.That(item.Cash, Is.True);
        Assert.That(item.Reward, Has.Length.EqualTo(1));
        Assert.That(item.Reward[0].ItemID, Is.EqualTo(4170012));
        Assert.That(item.Reward[0].Count, Is.EqualTo(12));
        Assert.That(item.Reward[0].Prob, Is.EqualTo(50));
        Assert.That(item.Reward[0].Period, Is.EqualTo(21600));
        Assert.That(item.Reward[0].Effect, Is.EqualTo("Effect/BasicEff/Event1/Success"));

        var item2 = provider.GetRequiredItem<CashPackagedItemTemplate>(5530001)!;
        Assert.That(item2.Reward, Has.Length.EqualTo(1));
        Assert.That(item2.Reward[0].ItemID, Is.EqualTo(1142144));
        Assert.That(item2.Reward[0].Count, Is.EqualTo(1));
        Assert.That(item2.Reward[0].Prob, Is.EqualTo(100));
    }

    [Test]
    public void WaterOfLifeItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var item = provider.GetRequiredItem<WaterOfLifeItemTemplate>(5180000)!;

        Assert.That(item.Life, Is.EqualTo(90));
        Assert.That(item.SlotMax, Is.EqualTo(1));
        Assert.That(item.Cash, Is.True);
    }

    [Test]
    public void InstallItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var bedItem = provider.GetRequiredItem<InstallItemTemplate>(3010000)!;
        Assert.That(bedItem.RecoveryHP, Is.EqualTo(50));
        Assert.That(bedItem.RecoveryMP, Is.EqualTo(0));
        Assert.That(bedItem.ReqLevel, Is.EqualTo(0));
        Assert.That(bedItem.Price, Is.EqualTo(100));
        Assert.That(bedItem.SlotMax, Is.EqualTo(1));
        Assert.That(bedItem.TradeBlock, Is.True);
        Assert.That(bedItem.NotSale, Is.True);

        var bedItem2 = provider.GetRequiredItem<InstallItemTemplate>(3010001)!;
        Assert.That(bedItem2.RecoveryHP, Is.EqualTo(35));
        Assert.That(bedItem2.ReqLevel, Is.EqualTo(6));
        Assert.That(bedItem2.Price, Is.EqualTo(500));
        Assert.That(bedItem2.SlotMax, Is.EqualTo(1));
    }

    [Test]
    public void EtcItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);

        var item = provider.GetRequiredItem<EtcItemTemplate>(4000113)!;
        Assert.That(item.lv, Is.EqualTo(34));

        var incubatorItem = provider.GetRequiredItem<IncubatorItemTemplate>(4220129)!;
        Assert.That(incubatorItem.Grade, Is.EqualTo(3));
        Assert.That(incubatorItem.QuestID, Is.EqualTo(8252));
        Assert.That(incubatorItem.ConsumeItems, Has.Some.Matches<IncubatorConsumeItem>(p => p.ItemId == 4032135 && p.Value == 1));
    }

    [Test]
    public void PetItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Item);
        var all = provider.LoadAll().OfType<PetItemTemplate>().ToList();

        var puff = all.First(p => p.TemplateId == 5000000);
        Assert.That(puff.Hungry, Is.EqualTo(2));
        Assert.That(puff.Permanent, Is.False);
        Assert.That(puff.Life, Is.EqualTo(90));
        Assert.That(puff.NoRevive, Is.False);
        Assert.That(puff.Cash, Is.True);
        Assert.That(puff.CanEvol, Is.False);
        Assert.That(puff.Evol1, Is.EqualTo(0));
        Assert.That(puff.Evol2, Is.EqualTo(0));
        Assert.That(puff.SlotMax, Is.EqualTo(1));

        var owl = all.First(p => p.TemplateId == 5000007);
        Assert.That(owl.Hungry, Is.EqualTo(5));
        Assert.That(owl.Life, Is.EqualTo(90));
        Assert.That(owl.Cash, Is.True);
        Assert.That(owl.SlotMax, Is.EqualTo(1));
    }

}

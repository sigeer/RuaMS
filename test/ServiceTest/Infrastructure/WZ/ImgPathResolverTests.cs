using Application.Templates.Reader;
using Application.Templates.Reader.Img;

namespace ServiceTest.Infrastructure.WZ;

[TestFixture("Img", Ignore = "仓库未关联img资源")]
internal class ImgPathResolverTests(string readerType) : WzTestBase(readerType)
{
    ImgPathResolver _resolver = null!;

    protected override void OnProviderRegistered()
    {
        _resolver = (_providerSource.Resolver as ImgPathResolver)!;
    }

    [Test]
    public void ResolveItem_ItemConsume_ReturnsConsumeGroupPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Item, 2000001);
        Assert.That(path, Is.EqualTo(Path.Combine("Item", "Consume", "0200.img")));
    }

    [Test]
    public void ResolveItem_ItemInstall_ReturnsInstallGroupPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Item, 3010000);
        Assert.That(path, Is.EqualTo(Path.Combine("Item", "Install", "0301.img")));
    }

    [Test]
    public void ResolveItem_ItemEtc_ReturnsEtcGroupPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Item, 4000000);
        Assert.That(path, Is.EqualTo(Path.Combine("Item", "Etc", "0400.img")));
    }

    [Test]
    public void ResolveItem_ItemCash_ReturnsCashGroupPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Item, 5281000);
        Assert.That(path, Is.EqualTo(Path.Combine("Item", "Cash", "0528.img")));
    }

    [Test]
    public void ResolveItem_ItemPet_ReturnsPetIndividualPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Item, 5000005);
        Assert.That(path, Is.EqualTo(Path.Combine("Item", "Pet", "5000005.img")));
    }

    [Test]
    public void ResolveItem_Map_ReturnsAreaBasedPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Map, 100000000);
        Assert.That(path, Is.EqualTo(Path.Combine("Map", "Map", "Map1", "100000000.img")));
    }

    [Test]
    public void ResolveItem_Map_ReturnsCorrectAreaForSmallMaps()
    {
        var path = _resolver.ResolveItem(ProviderType.Map, 980000101);
        Assert.That(path, Is.EqualTo(Path.Combine("Map", "Map", "Map9", "980000101.img")));
    }

    [Test]
    public void ResolveItem_Mob_Returns7DigitPaddedPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Mob, 8810018);
        Assert.That(path, Is.EqualTo(Path.Combine("Mob", "8810018.img")));
    }

    [Test]
    public void ResolveItem_SmallMob_Returns7DigitPaddedPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Mob, 9300187);
        Assert.That(path, Is.EqualTo(Path.Combine("Mob", "9300187.img")));
    }

    [Test]
    public void ResolveItem_Npc_Returns7DigitPaddedPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Npc, 1012000);
        Assert.That(path, Is.EqualTo(Path.Combine("Npc", "1012000.img")));
    }

    [Test]
    public void ResolveItem_Reactor_Returns7DigitPaddedPath()
    {
        var path = _resolver.ResolveItem(ProviderType.Reactor, 2001);
        Assert.That(path, Is.EqualTo(Path.Combine("Reactor", "0002001.img")));
    }

    [Test]
    public void ResolveItem_Skill_SmallJobCode()
    {
        var path = _resolver.ResolveItem(ProviderType.Skill, 1001004);
        Assert.That(path, Is.EqualTo(Path.Combine("Skill", "100.img")));
    }

    [Test]
    public void ResolveItem_Skill_LargeJobCode()
    {
        var path = _resolver.ResolveItem(ProviderType.Skill, 4121008);
        Assert.That(path, Is.EqualTo(Path.Combine("Skill", "412.img")));
    }

    [Test]
    public void ResolveItem_Skill_Zero()
    {
        var path = _resolver.ResolveItem(ProviderType.Skill, 0);
        Assert.That(path, Is.EqualTo(Path.Combine("Skill", "000.img")));
    }

    [Test]
    public void ResolveItem_Equip_ResolvesToCategorySubdirectory()
    {
        var path = _resolver.ResolveItem(ProviderType.Equip, 1002430);
        Assert.That(path, Is.EqualTo(Path.Combine("Character", "Cap", "01002430.img")));
    }

    [Test]
    public void ResolveGroup_EtcCashCommodity_ReturnsCommodityImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.EtcCashCommodity);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Etc", "Commodity.img")));
    }

    [Test]
    public void ResolveGroup_EtcCashPackage_ReturnsCashPackageImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.EtcCashPackage);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Etc", "CashPackage.img")));
    }

    [Test]
    public void ResolveGroup_EtcNpcLocation_ReturnsNpcLocationImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.EtcNpcLocation);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Etc", "NpcLocation.img")));
    }

    [Test]
    public void ResolveGroup_EtcScriptInfo_ReturnsScriptInfoImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.EtcScriptInfo);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Etc", "ScriptInfo.img")));
    }

    [Test]
    public void ResolveGroup_EtcMakeCharInfo_ReturnsMakeCharInfoImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.EtcMakeCharInfo);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Etc", "MakeCharInfo.img")));
    }

    [Test]
    public void ResolveGroup_MobSkill_ReturnsMobSkillImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.MobSkill);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Skill", "MobSkill.img")));
    }

    [Test]
    public void ResolveGroup_Quest_ReturnsThreeImgs()
    {
        var paths = _resolver.ResolveGroup(ProviderType.Quest);
        Assert.That(paths, Has.Length.EqualTo(3));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Quest", "QuestInfo.img")));
        Assert.That(paths[1], Does.EndWith(Path.Combine("Quest", "Act.img")));
        Assert.That(paths[2], Does.EndWith(Path.Combine("Quest", "Check.img")));
    }

    [Test]
    public void ResolveGroup_Quest_OrderIsPreserved()
    {
        var paths = _resolver.ResolveGroup(ProviderType.Quest);
        Assert.That(paths[0], Does.Contain("QuestInfo"));
        Assert.That(paths[1], Does.Contain("Act"));
        Assert.That(paths[2], Does.Contain("Check"));
    }

    [Test]
    public void ResolveGroup_UIMobWithBossHpBar_ReturnsUiWindowImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.UIMobWithBossHpBar);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("UI", "UIWindow.img")));
    }

    [Test]
    public void ResolveGroup_StringItem_ReturnsAllItemStringFiles()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringItem);
        Assert.That(paths, Has.Length.EqualTo(6));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Eqp.img")));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Consume.img")));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Cash.img")));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Ins.img")));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Etc.img")));
        Assert.That(paths, Has.Some.EndsWith(Path.Combine("String", "Pet.img")));
    }

    [Test]
    public void ResolveGroup_StringMap_ReturnsMapImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringMap);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("String", "Map.img")));
    }

    [Test]
    public void ResolveGroup_StringMob_ReturnsMobImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringMob);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("String", "Mob.img")));
    }

    [Test]
    public void ResolveGroup_StringNpc_ReturnsNpcImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringNpc);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("String", "Npc.img")));
    }

    [Test]
    public void ResolveGroup_StringQuest_ReturnsQuestInfoImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringQuest);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("Quest", "QuestInfo.img")));
    }

    [Test]
    public void ResolveGroup_StringSkill_ReturnsSkillImg()
    {
        var paths = _resolver.ResolveGroup(ProviderType.StringSkill);
        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.EndWith(Path.Combine("String", "Skill.img")));
    }
}

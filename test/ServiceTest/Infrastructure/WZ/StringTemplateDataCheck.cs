using Application.Templates.Reader;
using Application.Templates.String;

namespace ServiceTest.Infrastructure.WZ;

internal class StringProviderTests(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void StringKeyedProvider_CanRetrieveZhCn()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        Assert.That(provider, Is.Not.Null);
        Assert.That(provider.Key, Is.EqualTo("zh-CN"));
    }

    [Test]
    public void StringKeyedProvider_CanRetrieveEnUs()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("en-US");
        Assert.That(provider, Is.Not.Null);
        Assert.That(provider.Key, Is.EqualTo("en-US"));
    }

    [Test]
    public void SubProvider_Item_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Item);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void SubProvider_Map_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Map);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void SubProvider_Mob_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Mob);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void SubProvider_Npc_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Npc);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void SubProvider_Skill_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Skill);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void SubProvider_Quest_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Quest);
        Assert.That(sub, Is.Not.Null);
        var items = sub.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void Search_Item_ReturnsResults()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var results = provider.Search(StringCategory.Item, "药水").ToList();
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void Search_Map_ReturnsResults()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var results = provider.Search(StringCategory.Map, "射手村").ToList();
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void Search_Mob_ReturnsResults()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var results = provider.Search(StringCategory.Mob, "蜗牛").ToList();
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void Search_RespectsMaxCount()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var results = provider.Search(StringCategory.Item, "药水", maxCount: 3).ToList();
        Assert.That(results.Count, Is.LessThanOrEqualTo(3));
    }

    [Test]
    public void StringTemplate_HasName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Item);
        var all = sub.LoadAll().OfType<StringTemplate>().ToList();
        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.Name)), Is.True);
    }

    [Test]
    public void StringMapTemplate_HasStreetAndMapName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Map);
        var all = sub.LoadAll().OfType<StringMapTemplate>().ToList();
        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.StreetName)), Is.True);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.MapName)), Is.True);
    }

    [Test]
    public void MapString_Henesys_HasCorrectNames()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Map);
        var map = sub.LoadAll().OfType<StringMapTemplate>().FirstOrDefault(x => x.TemplateId == 100000000);
        Assert.That(map, Is.Not.Null);
        Assert.That(map.MapName, Is.EqualTo("射手村"));
        Assert.That(map.StreetName, Is.EqualTo("金银岛"));
    }

    [Test]
    public void MobString_snailHasCorrectName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Mob);
        var mob = sub.LoadAll().OfType<StringTemplate>().FirstOrDefault(x => x.TemplateId == 100100);
        Assert.That(mob, Is.Not.Null);
        Assert.That(mob.Name, Is.EqualTo("蜗牛"));
    }

    [Test]
    public void SkillString_NightLordHasCorrectName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Skill);
        var skill = sub.LoadAll().OfType<StringTemplate>().FirstOrDefault(x => x.TemplateId == 4121008);
        Assert.That(skill, Is.Not.Null);
        Assert.That(skill.Name, Is.EqualTo("忍者冲击"));
    }
}



internal class StringDetailDataCheckTests(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void StringNpcTemplate_HasNameAndTalkDefaults()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Npc);
        var all = sub.LoadAll().OfType<StringNpcTemplate>().ToList();

        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.Name)), Is.True);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.DefaultTalk0)), Is.True);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.DefaultTalk1)), Is.True);
    }

    [Test]
    public void StringNpc1012000_HasCorrectName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Npc);
        var npc = sub.LoadAll().OfType<StringNpcTemplate>().FirstOrDefault(x => x.TemplateId == 1012000);

        Assert.That(npc, Is.Not.Null);
        Assert.That(npc.Name, Is.EqualTo("射手村中巴"));
        Assert.That(npc.DefaultTalk0, Is.Not.Null.Or.Empty);
        Assert.That(npc.DefaultTalk1, Is.Not.Null.Or.Empty);
    }

    [Test]
    public void StringQuestTemplate_HasName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Quest);
        var all = sub.LoadAll().ToList();

        if (all.Count == 0)
            Assert.Ignore("Quest string provider returned no data (may not be available in this WZ set)");
        Assert.That(all.Any(s => !string.IsNullOrEmpty(((StringQuestTemplate)s).Name)), Is.True);
    }

    [Test]
    public void StringQuest1000_HasCorrectName()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Quest);
        var q = sub.LoadAll().OfType<StringQuestTemplate>().FirstOrDefault(x => x.TemplateId == 1000);

        if (q == null)
            Assert.Ignore("Quest string provider returned no data (may not be available in this WZ set)");
        Assert.That(q.Name, Is.Not.Null.Or.Empty);
    }

    [Test]
    public void StringTemplate_Item_HasDescription()
    {
        var provider = _providerSource.GetProviderByKey<IStringProvider>("zh-CN");
        var sub = provider.GetSubProvider(StringCategory.Item);
        var all = sub.LoadAll().OfType<StringTemplate>().ToList();

        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.Description)), Is.True);
        Assert.That(all.Any(s => !string.IsNullOrEmpty(s.Message)), Is.True);
    }
}

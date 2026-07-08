using Application.Templates.Quest;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class QuestTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void Quest1000_Info_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(1000)!;

        Assert.That(q.TemplateId, Is.EqualTo(1000));
        Assert.That(q.Info.Name, Is.Not.Null.Or.Empty);
        Assert.That(q.Info.Parent, Is.Not.Null.Or.Empty);
        Assert.That(q.Info.Area, Is.EqualTo(20));
        Assert.That(q.Info.AutoStart, Is.False);
        Assert.That(q.Info.AutoPreComplete, Is.False);
        Assert.That(q.Info.AutoComplete, Is.False);
        Assert.That(q.Info.TimeLimit, Is.EqualTo(0));
        Assert.That(q.Info.ViewMedalItem, Is.EqualTo(0));
    }

    [Test]
    public void Quest10000_Info_HasAutoFlags()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(10000)!;

        Assert.That(q.Info.AutoStart, Is.True);
        Assert.That(q.Info.AutoPreComplete, Is.True);
        Assert.That(q.Info.AutoComplete, Is.False);
        Assert.That(q.Info.Area, Is.EqualTo(50));
    }

    [Test]
    public void Quest10015_Info_HasAutoComplete()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(10015)!;

        Assert.That(q.Info.AutoComplete, Is.True);
        Assert.That(q.Info.Parent, Is.Not.Null.Or.Empty);
    }

    [Test]
    public void Quest1000_Check_HasStartAndEndDemands()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(1000)!;

        Assert.That(q.Check, Is.Not.Null);
        Assert.That(q.Check.StartDemand, Is.Not.Null);
        Assert.That(q.Check.EndDemand, Is.Not.Null);

        var start = q.Check.StartDemand;
        Assert.That(start.Npc, Is.EqualTo(2101));
        Assert.That(start.Job, Is.Not.Empty);
        Assert.That(start.Job[0], Is.EqualTo(0));

        Assert.That(start.DemandItem, Is.Not.Empty);
        Assert.That(start.DemandItem[0].ItemID, Is.EqualTo(1042003));
        Assert.That(start.DemandItem[0].Count, Is.EqualTo(1));

        var end = q.Check.EndDemand;
        Assert.That(end.Npc, Is.EqualTo(2100));
    }

    [Test]
    public void Quest1000_Act_HasEndAct()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(1000)!;

        Assert.That(q.Act, Is.Not.Null);
        Assert.That(q.Act.EndAct, Is.Not.Null);

        var endAct = q.Act.EndAct;
        Assert.That(endAct.NextQuest, Is.EqualTo(1001));
    }

    [Test]
    public void Quest1001_Act_HasItemsAndExp()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(1001)!;

        Assert.That(q.Act, Is.Not.Null);
        Assert.That(q.Act.StartAct, Is.Not.Null);
        Assert.That(q.Act.StartAct.Items, Is.Not.Empty);
        Assert.That(q.Act.StartAct.Items[0].ItemID, Is.EqualTo(4031003));
        Assert.That(q.Act.StartAct.Items[0].Count, Is.EqualTo(1));

        Assert.That(q.Act.EndAct, Is.Not.Null);
        Assert.That(q.Act.EndAct.Exp, Is.EqualTo(1));
        Assert.That(q.Act.EndAct.Items, Is.Not.Empty);
        Assert.That(q.Act.EndAct.Items[0].ItemID, Is.EqualTo(4031003));
        Assert.That(q.Act.EndAct.Items[0].Count, Is.EqualTo(-1));
    }

    [Test]
    public void NonExistentQuest_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var q = provider.GetItem(99999999);
        Assert.That(q, Is.Null);
    }

    [Test]
    public void LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.Quest);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var all = provider.LoadAll().OfType<QuestTemplate>().ToList();
        Assert.That(all.All(q => q.TemplateId > 0), Is.True);
    }

    [Test]
    public void LoadAll_AllHaveInfo()
    {
        var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        var all = provider.LoadAll().OfType<QuestTemplate>().ToList();
        Assert.That(all.All(q => q.Info != null), Is.True);
    }
}

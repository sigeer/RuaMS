using Application.Templates.Reader;
using Application.Templates.Skill;
using System.Drawing;

namespace ServiceTest.Infrastructure.WZ;

internal class MobSkillProviderTests(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.MobSkill);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var all = provider.LoadAll().OfType<MobSkillTemplate>().ToList();
        Assert.That(all.All(m => m.TemplateId > 0), Is.True);
    }

    [Test]
    public void GetItem_100_HasLevelData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        Assert.That(skill.TemplateId, Is.EqualTo(100));
        Assert.That(skill.LevelData, Is.Not.Empty);

        var level1 = skill.GetLevelData(1);
        Assert.That(level1, Is.Not.Null);
        Assert.That(level1.nSLV, Is.EqualTo(1));
        Assert.That(level1.X, Is.EqualTo(115));
        Assert.That(level1.Interval, Is.EqualTo(40));
        Assert.That(level1.Time, Is.EqualTo(30));
    }

    [Test]
    public void GetItem_120_HasLevelData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(120)!;

        Assert.That(skill.TemplateId, Is.EqualTo(120));
        Assert.That(skill.LevelData, Is.Not.Empty);

        var level1 = skill.GetLevelData(1);
        Assert.That(level1, Is.Not.Null);
        Assert.That(level1.nSLV, Is.EqualTo(1));
        Assert.That(level1.Interval, Is.EqualTo(10));
        Assert.That(level1.Time, Is.EqualTo(30));
        Assert.That(level1.MpCon, Is.EqualTo(5));
        Assert.That(level1.Prop, Is.EqualTo(50));
        Assert.That(level1.Lt, Is.EqualTo(new Point(-300, -120)));
        Assert.That(level1.Rb, Is.EqualTo(new Point(300, 120)));
        Assert.That(level1.HP, Is.EqualTo(100));
        Assert.That(level1.SummonEffect, Is.EqualTo(0));
        Assert.That(level1.Limit, Is.EqualTo(0));
        Assert.That(level1.Count, Is.EqualTo(1));
        Assert.That(level1.SummonIDs, Is.Empty);
    }

    [Test]
    public void GetItem_200_HasSummonAndHpLevelData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(200)!;

        Assert.That(skill.TemplateId, Is.EqualTo(200));
        Assert.That(skill.LevelData, Is.Not.Empty);

        var level1 = skill.GetLevelData(1);
        Assert.That(level1, Is.Not.Null);
        Assert.That(level1.nSLV, Is.EqualTo(1));
        Assert.That(level1.Interval, Is.EqualTo(45));
        Assert.That(level1.HP, Is.EqualTo(75));
        Assert.That(level1.Limit, Is.EqualTo(5));
        Assert.That(level1.SummonEffect, Is.EqualTo(2));
        Assert.That(level1.SummonIDs, Is.EqualTo(new[] { 7130200 }));
    }

    [Test]
    public void NonExistentMobSkill_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var item = provider.GetItem(-1);
        Assert.That(item, Is.Null);
    }

    [Test]
    public void GetItem_100_LevelData_HasCompleteProperties()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        var level1 = skill.GetLevelData(1);
        Assert.That(level1, Is.Not.Null);
        Assert.That(level1.MpCon, Is.EqualTo(5));
        Assert.That(level1.X, Is.EqualTo(115));
        Assert.That(level1.Interval, Is.EqualTo(40));
        Assert.That(level1.Time, Is.EqualTo(30));
    }
}

using Application.Templates.Reader;
using Application.Templates.Skill;
using System.Drawing;

namespace ServiceTest.Infrastructure.WZ;

internal class SkillTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void SkillProvider_LoadAll_ReturnsNonEmptyCollection()
    {
        var provider = _providerSource.GetProvider(ProviderType.Skill);
        var allSkills = provider.LoadAll().ToList();
        Assert.That(allSkills, Is.Not.Empty);
    }

    [Test]
    public void SkillProvider_LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<SkillTemplate>>(ProviderType.Skill);
        var allSkills = provider.LoadAll().OfType<SkillTemplate>().ToList();
        Assert.That(allSkills.All(s => s.TemplateId > 0), Is.True);
    }

    [Test]
    public void NightLordStorm_HasLevelData()
    {
        var provider = _providerSource.GetProvider<IProvider<SkillTemplate>>(ProviderType.Skill);
        var skill = provider.GetItem(4121008)!;

        Assert.That(skill.TemplateId, Is.EqualTo(4121008));
        Assert.That(skill.LevelData, Is.Not.Empty);

        var level1 = skill.LevelData[0];
        Assert.That(level1.Level, Is.EqualTo(1));
        Assert.That(level1.MpCon, Is.EqualTo(16));
        Assert.That(level1.Damage, Is.EqualTo(42));
        Assert.That(level1.Prop, Is.EqualTo(42));
        Assert.That(level1.MobCount, Is.EqualTo(6));
        Assert.That(level1.Time, Is.EqualTo(1));
        Assert.That(level1.LeftTop, Is.EqualTo(new Point(-100, -70)));
        Assert.That(level1.RightBottom, Is.EqualTo(new Point(100, 30)));

        var level30 = skill.LevelData[29];
        Assert.That(level30.Level, Is.EqualTo(30));
        Assert.That(level30.MpCon, Is.EqualTo(25));
        Assert.That(level30.Damage, Is.EqualTo(80));
        Assert.That(level30.Prop, Is.EqualTo(100));
        Assert.That(level30.MobCount, Is.EqualTo(6));
        Assert.That(level30.Time, Is.EqualTo(1));
        Assert.That(level30.LeftTop, Is.EqualTo(new Point(-200, -70)));
        Assert.That(level30.RightBottom, Is.EqualTo(new Point(200, 30)));
    }

    [Test]
    public void NightLordStorm_HasActionData()
    {
        var provider = _providerSource.GetProvider<IProvider<SkillTemplate>>(ProviderType.Skill);
        var skill = provider.GetItem(4121008)!;

        Assert.That(skill.ActionData, Is.Not.Null);
        Assert.That(skill.ActionData.Str0, Is.EqualTo("ninjastorm"));
    }

    [Test]
    public void NightLordStorm_HasEffectData()
    {
        var provider = _providerSource.GetProvider<IProvider<SkillTemplate>>(ProviderType.Skill);
        var skill = provider.GetItem(4121008)!;

        Assert.That(skill.EffectData, Is.Not.Empty);
        Assert.That(skill.EffectData[0].Delay, Is.EqualTo(60));
    }

    [Test]
    public void NightLordStorm_SpecialMarkers_WorkCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<SkillTemplate>>(ProviderType.Skill);
        var skill = provider.GetItem(4121008)!;

        Assert.That(skill.HasHitNode, Is.True);
        Assert.That(skill.HasBallNode, Is.False);
        Assert.That(skill.HasSummonNode, Is.False);

        Assert.That(skill.LevelData, Has.Length.EqualTo(30));
        Assert.That(skill.LevelData[0].Level, Is.EqualTo(1));
        Assert.That(skill.LevelData[10].Level, Is.EqualTo(11));
        Assert.That(skill.LevelData[29].Level, Is.EqualTo(30));
    }
}



internal class SkillDetailDataCheckTests(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void MobSkill100_HasLevelData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        Assert.That(skill, Is.Not.Null);
        Assert.That(skill.TemplateId, Is.EqualTo(100));
        Assert.That(skill.LevelData, Is.Not.Empty);

        var level1 = skill.LevelData[0];
        Assert.That(level1.nSLV, Is.EqualTo(1));
        Assert.That(level1.Interval, Is.EqualTo(40));
        Assert.That(level1.MpCon, Is.EqualTo(5));
        Assert.That(level1.Time, Is.EqualTo(30));
        Assert.That(level1.X, Is.EqualTo(115));
    }

    [Test]
    public void MobSkill100_SecondLevel_HasCorrectValues()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        Assert.That(skill.LevelData.Length, Is.GreaterThanOrEqualTo(2));
        var level2 = skill.LevelData[1];
        Assert.That(level2.nSLV, Is.EqualTo(2));
        Assert.That(level2.X, Is.EqualTo(130));
        Assert.That(level2.MpCon, Is.EqualTo(5));
        Assert.That(level2.Interval, Is.EqualTo(40));
        Assert.That(level2.Time, Is.EqualTo(30));
    }

    [Test]
    public void MobSkill100_HasDefaultProperties()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        foreach (var level in skill.LevelData)
        {
            Assert.That(level.Y, Is.GreaterThan(0));
            Assert.That(level.Prop, Is.EqualTo(0));
            Assert.That(level.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public void MobSkill100_Level4_HasSpecificValues()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        var skill = provider.GetItem(100)!;

        var level4 = skill.LevelData[3];
        Assert.That(level4.nSLV, Is.EqualTo(4));
        Assert.That(level4.X, Is.EqualTo(100));
        Assert.That(level4.Y, Is.EqualTo(1));
    }

    [Test]
    public void MobSkillProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.MobSkill);
        var all = provider.LoadAll().OfType<MobSkillTemplate>().ToList();
        Assert.That(all, Is.Not.Empty);
    }

    [Test]
    public void NonExistentMobSkill_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MobSkillTemplate>>(ProviderType.MobSkill);
        Assert.That(provider.GetItem(999999), Is.Null);
    }
}

using Application.Templates.Reader.Img.Provider;
using Application.Templates.Mob;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class MobTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void HorntailMob_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(8810018)!;

        Assert.That(mob.TemplateId, Is.EqualTo(8810018));
        Assert.That(mob.Level, Is.EqualTo(160));
        Assert.That(mob.MaxHP, Is.EqualTo(2090000000));
        Assert.That(mob.MaxMP, Is.EqualTo(0));
        Assert.That(mob.Boss, Is.True);
        Assert.That(mob.PublicReward, Is.EqualTo(1));
        Assert.That(mob.ExplosiveReward, Is.True);
        Assert.That(mob.HpTagColor, Is.EqualTo(3));
        Assert.That(mob.HpTagBgColor, Is.EqualTo(5));
        Assert.That(mob.UnDead, Is.False);
        Assert.That(mob.Exp, Is.EqualTo(0));
        Assert.That(mob.PAD, Is.EqualTo(0));
        Assert.That(mob.PDD, Is.EqualTo(0));
        Assert.That(mob.MAD, Is.EqualTo(0));
        Assert.That(mob.MDD, Is.EqualTo(0));
        Assert.That(mob.ACC, Is.EqualTo(0));
        Assert.That(mob.EVA, Is.EqualTo(0));
        Assert.That(mob.Buff, Is.EqualTo(2022108));
    }

    [Test]
    public void CatchMob_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(9300187)!;

        Assert.That(mob.TemplateId, Is.EqualTo(9300187));
        Assert.That(mob.Level, Is.EqualTo(40));
        Assert.That(mob.MaxHP, Is.EqualTo(8000));
        Assert.That(mob.MaxMP, Is.EqualTo(100));
        Assert.That(mob.Exp, Is.EqualTo(800));
        Assert.That(mob.PAD, Is.EqualTo(130));
        Assert.That(mob.PDD, Is.EqualTo(160));
        Assert.That(mob.MAD, Is.EqualTo(165));
        Assert.That(mob.MDD, Is.EqualTo(160));
        Assert.That(mob.ACC, Is.EqualTo(140));
        Assert.That(mob.EVA, Is.EqualTo(10));
        Assert.That(mob.Boss, Is.True);
        Assert.That(mob.UnDead, Is.False);
        Assert.That(mob.ExplosiveReward, Is.True);
        Assert.That(mob.Link, Is.EqualTo(9300003));
    }

    [Test]
    public void CatchMob_HasSkillData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(9300187)!;

        Assert.That(mob.Skill, Is.Not.Empty);
        var skill0 = mob.Skill[0];
        Assert.That(skill0.Skill, Is.EqualTo(200));
        Assert.That(skill0.Action, Is.EqualTo(1));
        Assert.That(skill0.Level, Is.EqualTo(106));
        Assert.That(skill0.EffectAfter, Is.EqualTo(0));
    }

    [Test]
    public void Mob5100002_HasSelfDestruction()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(5100002)!;

        Assert.That(mob.SelfDestruction, Is.Not.Null);
        Assert.That(mob.SelfDestruction.ActionType, Is.EqualTo(1));
        Assert.That(mob.SelfDestruction.Hp, Is.EqualTo(1800));
        Assert.That(mob.SelfDestruction.RemoveAfter, Is.EqualTo(-1));
    }

    [Test]
    public void Mob9300166_HasSelfDestructionAndLosedItems()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(9300166)!;

        Assert.That(mob.SelfDestruction, Is.Not.Null);
        Assert.That(mob.SelfDestruction.ActionType, Is.EqualTo(4));
        Assert.That(mob.SelfDestruction.RemoveAfter, Is.EqualTo(0));
        Assert.That(mob.SelfDestruction.Hp, Is.EqualTo(-1));

        Assert.That(mob.LosedItems, Is.Not.Empty);
        var loseItem = mob.LosedItems[0];
        Assert.That(loseItem.Id, Is.EqualTo(4031868));
        Assert.That(loseItem.Prop, Is.EqualTo(40));
        Assert.That(loseItem.X, Is.EqualTo(5));
    }

    [Test]
    public void Mob5090000_HasBanData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(5090000)!;

        Assert.That(mob.Ban, Is.Not.Null);
        Assert.That(mob.Ban.Message, Is.Not.Null.Or.Empty);
        Assert.That(mob.Ban.Map, Is.EqualTo(103000100));
        Assert.That(mob.Ban.PortalName, Is.EqualTo("sp"));
    }

    [Test]
    public void Mob2220000_HasSpeakInfo()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(2220000)!;

        Assert.That(mob.SpeakInfos, Is.Not.Empty);
        var speak = mob.SpeakInfos[0];
        Assert.That(speak.Hp, Is.EqualTo(1200));
    }

    [Test]
    public void Mob8180000_HasAttackWithDeadlyAttack()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(8180000)!;

        Assert.That(mob.AttackInfos, Is.Not.Empty);
        var attack = mob.AttackInfos.FirstOrDefault(a => a.DeadlyAttack);
        Assert.That(attack, Is.Not.Null);
        Assert.That(attack.AttackAfter, Is.EqualTo(1750));
        Assert.That(attack.ConMP, Is.EqualTo(1));
    }

    [Test]
    public void Mob9300187_HasSkillData()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(9300187)!;

        Assert.That(mob.Skill, Is.Not.Empty);
        var skill0 = mob.Skill[0];
        Assert.That(skill0.Skill, Is.EqualTo(200));
        Assert.That(skill0.Action, Is.EqualTo(1));
        Assert.That(skill0.Level, Is.EqualTo(106));
    }

    [Test]
    public void Mob5090000_HasBossAndUndead()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var mob = provider.GetItem(5090000)!;

        Assert.That(mob.Boss, Is.True);
        Assert.That(mob.UnDead, Is.True);
        Assert.That(mob.Level, Is.EqualTo(56));
        Assert.That(mob.MaxHP, Is.EqualTo(30000));
        Assert.That(mob.Exp, Is.EqualTo(5600));
    }

    [Test]
    public void MobProvider_LoadAll_ReturnsNonEmptyCollection()
    {
        var provider = _providerSource.GetProvider(ProviderType.Mob);
        var allMobs = provider.LoadAll().ToList();
        Assert.That(allMobs, Is.Not.Empty);
    }

    [Test]
    public void MobProvider_LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        var allMobs = provider.LoadAll().OfType<MobTemplate>().ToList();
        Assert.That(allMobs.All(m => m.TemplateId > 0), Is.True);
    }
}

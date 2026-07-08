using Application.Templates.PartyQuest;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class PartyQuestTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void CarnivalSkillProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.CarnivalSkill);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void CarnivalSkillProvider_GetItem_ReturnsCorrect()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        var item = provider.GetItem(0);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.SpendCP, Is.EqualTo(14));
        Assert.That(item.MobSkillId, Is.EqualTo(120));
        Assert.That(item.Level, Is.EqualTo(10));
        Assert.That(item.TargetsAll, Is.False);
    }

    [Test]
    public void CarnivalSkillProvider_GetItem_MultiTarget()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        var item = provider.GetItem(1);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.SpendCP, Is.EqualTo(17));
        Assert.That(item.MobSkillId, Is.EqualTo(121));
        Assert.That(item.Level, Is.EqualTo(6));
        Assert.That(item.TargetsAll, Is.True);
    }

    [Test]
    public void CarnivalSkillProvider_GetItem_NonExistent_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        var item = provider.GetItem(-1);
        Assert.That(item, Is.Null);
    }

    [Test]
    public void CarnivalGuardianProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.CarnivalGuardian);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void CarnivalGuardianProvider_GetItem_ReturnsCorrect()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalGuardianTemplate>>(ProviderType.CarnivalGuardian);
        var item = provider.GetItem(0);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.SpendCP, Is.EqualTo(17));
        Assert.That(item.MobSkillId, Is.EqualTo(150));
        Assert.That(item.Level, Is.EqualTo(1));
    }

    [Test]
    public void CarnivalGuardianProvider_GetItem_NonExistent_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalGuardianTemplate>>(ProviderType.CarnivalGuardian);
        var item = provider.GetItem(-1);
        Assert.That(item, Is.Null);
    }

    [Test]
    public void CarnivalSkill_HasRequiredProperties()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        var all = provider.LoadAll().ToList();
        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => s.SpendCP > 0), Is.True);
        Assert.That(all.Any(s => s.MobSkillId > 0), Is.True);
        Assert.That(all.Any(s => s.Level > 0), Is.True);
    }

    [Test]
    public void CarnivalGuardian_HasRequiredProperties()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalGuardianTemplate>>(ProviderType.CarnivalGuardian);
        var all = provider.LoadAll().ToList();
        Assert.That(all, Is.Not.Empty);
        Assert.That(all.Any(s => s.SpendCP > 0), Is.True);
        Assert.That(all.Any(s => s.MobSkillId > 0), Is.True);
    }

    [Test]
    public void CarnivalSkill_HasTargetsAllFlag()
    {
        var provider = _providerSource.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        var all = provider.LoadAll().ToList();
        Assert.That(all.Any(s => s.TargetsAll), Is.True);
        Assert.That(all.Any(s => !s.TargetsAll), Is.True);
    }
}

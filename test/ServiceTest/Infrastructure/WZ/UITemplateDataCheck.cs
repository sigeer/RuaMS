using Application.Templates.Reader;
using Application.Templates.UI;

namespace ServiceTest.Infrastructure.WZ;

internal class UITemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void MobWithBossHpBarProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.UIMobWithBossHpBar);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void MobWithBossHpBarProvider_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<MobWithBossHpBarTemplate>>(ProviderType.UIMobWithBossHpBar);
        var all = provider.LoadAll().OfType<MobWithBossHpBarTemplate>().ToList();
        Assert.That(all.All(m => m.TemplateId > 0), Is.True);
    }
}

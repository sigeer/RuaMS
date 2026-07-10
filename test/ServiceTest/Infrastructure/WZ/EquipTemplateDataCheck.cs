using Application.Templates;
using Application.Templates.Character;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class EquipTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.Equip);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider(ProviderType.Equip);
        var all = provider.LoadAll().ToList();
        Assert.That(all.All(e => e.TemplateId > 0), Is.True);
    }

    [Test]
    public void NonExistentEquip_ReturnsNull()
    {
        var provider = _providerSource.GetProvider(ProviderType.Equip);
        var item = provider.GetItem(-1);
        Assert.That(item, Is.Null);
    }

    [Test]
    public void EquipItemTemplateDataCheck()
    {
        var provider = _providerSource.GetProvider<IProvider<AbstractItemTemplate>>(ProviderType.Equip);
        var item = provider.GetRequiredItem<EquipTemplate>(01002430)!;

        Assert.That(item.TemplateId, Is.EqualTo(1002430));
        Assert.That(item.ReqLevel, Is.EqualTo(60));
        Assert.That(item.Islot, Is.EqualTo("Cp"));
        Assert.That(item.TUC, Is.EqualTo(7));
        Assert.That(item.IncSTR, Is.EqualTo(6));
        Assert.That(item.IncDEX, Is.EqualTo(6));
        Assert.That(item.IncINT, Is.EqualTo(6));
        Assert.That(item.IncLUK, Is.EqualTo(6));
        Assert.That(item.IncPDD, Is.EqualTo(88));
        Assert.That(item.IncMDD, Is.EqualTo(51));
        Assert.That(item.IncACC, Is.EqualTo(6));
        Assert.That(item.IncEVA, Is.EqualTo(6));
        Assert.That(item.Price, Is.EqualTo(350000));
        Assert.That(item.ReqPOP, Is.EqualTo(88));
        Assert.That(item.IncPAD, Is.EqualTo(0));
        Assert.That(item.IncMAD, Is.EqualTo(0));
        Assert.That(item.IncMHP, Is.EqualTo(0));
        Assert.That(item.IncMMP, Is.EqualTo(0));
        Assert.That(item.IncSpeed, Is.EqualTo(0));
        Assert.That(item.IncJump, Is.EqualTo(0));
        Assert.That(item.incCraft, Is.EqualTo(0));
        Assert.That(item.EquipTradeBlock, Is.False);
        Assert.That(item.ReqJob, Is.EqualTo(0));
        Assert.That(item.ReqSTR, Is.EqualTo(0));
        Assert.That(item.ReqDEX, Is.EqualTo(0));
        Assert.That(item.ReqINT, Is.EqualTo(0));
        Assert.That(item.ReqLUK, Is.EqualTo(0));
        Assert.That(item.Fs, Is.EqualTo(0));
        Assert.That(item.SlotMax, Is.EqualTo(1));
        Assert.That(item.Cash, Is.False);
        Assert.That(item.LevelData, Has.Length.EqualTo(30));
        Assert.That(item.LevelData[0].Level, Is.EqualTo(1));
        Assert.That(item.LevelData[0].FieldCount, Is.EqualTo(1));
        Assert.That(item.LevelData[0].Exp, Is.EqualTo(10000));
        Assert.That(item.LevelData[1].Level, Is.EqualTo(2));
        Assert.That(item.LevelData[1].Exp, Is.EqualTo(10000));
        Assert.That(item.LevelData[29].Level, Is.EqualTo(30));
        Assert.That(item.LevelData[29].Exp, Is.EqualTo(10000));
        Assert.That(item.IsElemental, Is.False);
    }
}

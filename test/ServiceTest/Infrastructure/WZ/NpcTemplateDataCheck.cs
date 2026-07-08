using Application.Templates.Reader.Img.Provider;
using Application.Templates.Npc;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class NpcTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void RegularNpc_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
        var npc = provider.GetItem(1012000)!;

        Assert.That(npc.TemplateId, Is.EqualTo(1012000));
        Assert.That(npc.Script, Is.EqualTo("taxi2"));
        Assert.That(npc.MapleTV, Is.False);
        Assert.That(npc.StoreBank, Is.False);
        Assert.That(npc.Parcel, Is.False);
        Assert.That(npc.GuildRank, Is.False);
    }

    [Test]
    public void StorageNpc_HasStorageProperties()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
        var npc = provider.GetItem(1012000)!;

        Assert.That(npc.TrunkPut, Is.Null);
        Assert.That(npc.TrunkGet, Is.Null);
    }

    [Test]
    public void StorageNpc_1012006_HasTrunkData()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
        var npc = provider.GetItem(1012006)!;

        Assert.That(npc.TemplateId, Is.EqualTo(1012006));
        Assert.That(npc.TrunkPut, Is.Null);
        Assert.That(npc.TrunkGet, Is.Null);
        Assert.That(npc.Script, Is.Not.Null);
    }

    [Test]
    public void NonExistentNpc_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
        var npc = provider.GetItem(9999999);
        Assert.That(npc, Is.Null);
    }

    [Test]
    public void NpcProvider_LoadAll_ReturnsNonEmptyCollection()
    {
        var provider = _providerSource.GetProvider(ProviderType.Npc);
        var allNpcs = provider.LoadAll().ToList();
        Assert.That(allNpcs, Is.Not.Empty);
    }

    [Test]
    public void NpcProvider_LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
        var allNpcs = provider.LoadAll().OfType<NpcTemplate>().ToList();
        Assert.That(allNpcs.All(n => n.TemplateId > 0), Is.True);
    }
}

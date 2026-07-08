using Application.Templates.Reader.Img.Provider;
using Application.Templates.Reactor;
using Application.Templates.Reader;
using System.Drawing;

namespace ServiceTest.Infrastructure.WZ;

internal class ReactorTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    [Test]
    public void GetItem_9002001_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<ReactorTemplate>>(ProviderType.Reactor);
        var reactor = provider.GetItem(9002001)!;

        Assert.That(reactor.TemplateId, Is.EqualTo(9002001));
        Assert.That(reactor.Action, Is.EqualTo("eventItem1"));
        Assert.That(reactor.ActivateByTouch, Is.False);
    }

    [Test]
    public void GetItem_2001_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<ReactorTemplate>>(ProviderType.Reactor);
        var reactor = provider.GetItem(2001)!;

        Assert.That(reactor.TemplateId, Is.EqualTo(2001));
        Assert.That(reactor.Action, Is.EqualTo("mBoxItem0"));
        Assert.That(reactor.StateInfoList, Is.Not.Empty);
    }

    [Test]
    public void GetItem_2008006_HasStateInfoWithEvents()
    {
        var provider = _providerSource.GetProvider<IProvider<ReactorTemplate>>(ProviderType.Reactor);
        var reactor = provider.GetItem(2008006)!;

        Assert.That(reactor.TemplateId, Is.EqualTo(2008006));
        Assert.That(reactor.Action, Is.Null);
        Assert.That(reactor.ActivateByTouch, Is.False);
        Assert.That(reactor.StateInfoList, Has.Length.EqualTo(8));

        var state0 = reactor.StateInfoList[0];
        Assert.That(state0.State, Is.EqualTo(0));
        Assert.That(state0.TimeOut, Is.EqualTo(-1));
        Assert.That(state0.EventInfos, Has.Length.EqualTo(7));

        var event0 = state0.EventInfos[0];
        Assert.That(event0.EventType, Is.EqualTo(100));
        Assert.That(event0.NextState, Is.EqualTo(1));
        Assert.That(event0.Int0Value, Is.EqualTo(4001056));
        Assert.That(event0.Int1Value, Is.EqualTo(1));
        Assert.That(event0.Int2Value, Is.EqualTo(0));
        Assert.That(event0.Lt, Is.EqualTo(new Point(-52, -64)));
        Assert.That(event0.Rb, Is.EqualTo(new Point(40, 79)));
        Assert.That(event0.ActiveSkillId, Is.Null);

        var event1 = state0.EventInfos[1];
        Assert.That(event1.EventType, Is.EqualTo(100));
        Assert.That(event1.NextState, Is.EqualTo(2));
        Assert.That(event1.Int0Value, Is.EqualTo(4001057));
        Assert.That(event1.Int1Value, Is.EqualTo(1));
        Assert.That(event1.Lt, Is.EqualTo(new Point(-52, -64)));
        Assert.That(event1.Rb, Is.EqualTo(new Point(40, 79)));

        var state1 = reactor.StateInfoList[1];
        Assert.That(state1.State, Is.EqualTo(1));
    }

    [Test]
    public void ReactorProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.Reactor);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void ReactorProvider_LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider(ProviderType.Reactor);
        var all = provider.LoadAll().OfType<ReactorTemplate>().ToList();
        Assert.That(all.All(r => r.TemplateId > 0), Is.True);
    }

    [Test]
    public void NonExistentReactor_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<ReactorTemplate>>(ProviderType.Reactor);
        var reactor = provider.GetItem(-1);
        Assert.That(reactor, Is.Null);
    }
}

using Application.Templates.Etc;
using Application.Templates.Reader;

namespace ServiceTest.Infrastructure.WZ;

internal class EtcTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    private IProvider<MakerCharInfoTemplate>? TryGetMakerCharInfoProvider()
    {
        try
        {
            return _providerSource.GetProvider<IProvider<MakerCharInfoTemplate>>(ProviderType.EtcMakeCharInfo);
        }
        catch (Application.Templates.Exceptions.ProviderNotFoundException)
        {
            return null;
        }
    }

    [Test]
    public void CashCommodityProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.EtcCashCommodity);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void CashCommodity_GetItem_10000000_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<CashCommodityTemplate>>(ProviderType.EtcCashCommodity);
        var item = provider.GetItem(10000000);

        Assert.That(item, Is.Not.Null);
        Assert.That(item.CashItemSN, Is.EqualTo(10000000));
        Assert.That(item.ItemID, Is.EqualTo(1002000));
        Assert.That(item.Count, Is.EqualTo(1));
        Assert.That(item.Price, Is.EqualTo(300));
        Assert.That(item.Period, Is.EqualTo(14));
        Assert.That(item.Priority, Is.EqualTo(8));
        Assert.That(item.Gender, Is.EqualTo(2));
        Assert.That(item.OnSale, Is.False);
        Assert.That(item.Classification, Is.EqualTo(0));
    }

    [Test]
    public void CashPackageProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.EtcCashPackage);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void CashPackageProvider_GetItem_ReturnsCorrect()
    {
        var provider = _providerSource.GetProvider<IProvider<CashPackageTemplate>>(ProviderType.EtcCashPackage);
        var item = provider.GetItem(9100000);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.SNList, Is.Not.Null);
        Assert.That(item.SNList, Is.Not.Empty);
    }

    [Test]
    public void NpcLocationProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.EtcNpcLocation);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void NpcLocationProvider_GetItem_ReturnsCorrect()
    {
        var provider = _providerSource.GetProvider<IProvider<NpcLocationTemplate>>(ProviderType.EtcNpcLocation);
        var items = provider.LoadAll().OfType<NpcLocationTemplate>().ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(items.Any(i => i.Maps.Length > 0), Is.True);
    }

    [Test]
    public void ScriptInfoProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.EtcScriptInfo);
        var items = provider.LoadAll().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public void ScriptInfoProvider_GetItem_ReturnsCorrect()
    {
        var provider = _providerSource.GetProvider<IProvider<ScriptInfoTemplate>>(ProviderType.EtcScriptInfo);
        var items = provider.LoadAll().OfType<ScriptInfoTemplate>().ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(items.Any(i => !string.IsNullOrEmpty(i.Name)), Is.True);
        Assert.That(items.Any(i => !string.IsNullOrEmpty(i.Value)), Is.True);
    }

    [Test]
    public void MakerCharInfo_CharMale_HasFaceAndHair()
    {
        var provider = TryGetMakerCharInfoProvider();
        if (provider == null)
            Assert.Ignore("EtcMakeCharInfo provider not registered");
        var maker = provider.GetItem(0)!;

        Assert.That(maker, Is.Not.Null);
        Assert.That(maker.CharMale, Is.Not.Null);
        Assert.That(maker.CharMale.FaceIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.FaceIdArray[0], Is.EqualTo(20000));
        Assert.That(maker.CharMale.HairIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.HairIdArray[0], Is.EqualTo(30030));

        Assert.That(maker.CharMale.HairColorIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.SkinIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.TopIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.BottomIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.ShoeIdArray, Is.Not.Empty);
        Assert.That(maker.CharMale.WeaponIdArray, Is.Not.Empty);
    }

    [Test]
    public void MakerCharInfo_CharFemale_HasCorrectData()
    {
        var provider = TryGetMakerCharInfoProvider();
        if (provider == null)
            Assert.Ignore("EtcMakeCharInfo provider not registered");
        var maker = provider.GetItem(0)!;

        Assert.That(maker.CharFemale, Is.Not.Null);
        Assert.That(maker.CharFemale.FaceIdArray, Is.Not.Empty);
        Assert.That(maker.CharFemale.FaceIdArray[0], Is.EqualTo(21000));
        Assert.That(maker.CharFemale.HairIdArray[0], Is.EqualTo(31000));
    }

    [Test]
    public void MakerCharInfo_PremiumAndOrient_HaveData()
    {
        var provider = TryGetMakerCharInfoProvider();
        if (provider == null)
            Assert.Ignore("EtcMakeCharInfo provider not registered");
        var maker = provider.GetItem(0)!;

        Assert.That(maker.PremiumCharMale, Is.Not.Null);
        Assert.That(maker.PremiumCharFemale, Is.Not.Null);
        Assert.That(maker.OrientCharMale, Is.Not.Null);
        Assert.That(maker.OrientCharFemale, Is.Not.Null);
    }

    [Test]
    public void OxQuiz1_HasQuestions()
    {
        var provider = _providerSource.GetProvider<IProvider<OxQuizTemplate>>(ProviderType.OxQuiz);
        var quiz = provider.GetItem(1)!;

        Assert.That(quiz, Is.Not.Null);
        Assert.That(quiz.Questions, Is.Not.Empty);

        var seventh = quiz.Questions[6];
        Assert.That(seventh.Answer, Is.EqualTo(1));
    }

    [Test]
    public void GetItem_FirstQuestion_HasExpectedAnswer()
    {
        var provider = _providerSource.GetProvider<IProvider<OxQuizTemplate>>(ProviderType.OxQuiz);
        var item = provider.GetItem(1);
        Assert.That(item, Is.Not.Null);
        var q1 = item.Questions.FirstOrDefault(q => q.QuestionId == 1);
        Assert.That(q1, Is.Not.Null);
        Assert.That(q1.Answer, Is.EqualTo(0));
    }

    [Test]
    public void OxQuizProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.OxQuiz);
        Assert.That(provider.LoadAll().ToList(), Is.Not.Empty);
    }

    [Test]
    public void NonExistentOxQuiz_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<OxQuizTemplate>>(ProviderType.OxQuiz);
        Assert.That(provider.GetItem(-1), Is.Null);
    }
}

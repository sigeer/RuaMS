using Application.Templates.XmlWzReader.Provider;
using server.life;

namespace ServiceTest.Infrastructure.WZ
{
    internal class LifeFactoryTests : WzTestBase
    {

        protected override void OnProviderRegistering()
        {
            _providerSource.RegisterProvider<MobWithBossHpBarProvider>(o => new MobWithBossHpBarProvider(o));
        }

        [Test]
        public void GetHpBarBossesTest()
        {
            Assert.That(_providerSource.GetProvider<MobWithBossHpBarProvider>().LoadAll().Select(x => x.TemplateId).ToHashSet(),
                Is.EqualTo(LifeFactory.getHpBarBosses()));
        }
    }
}

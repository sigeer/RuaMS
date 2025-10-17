using Application.Resources;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using System.Globalization;
using XmlWzReader.wz;

namespace ServiceTest.Infrastructure.WZ
{
    [TestFixture("en-US")]
    [TestFixture("zh-CN")]
    internal class StringTests: WzTestBase
    {
        CultureInfo testCulture;
        public StringTests(string currentCulture)
        {
            testCulture = CultureInfo.GetCultureInfo(currentCulture);
        }

        protected override void OnProviderRegistering()
        {
            ProviderFactory.ConfigureWith(o =>
            {
                o.RegisterProvider<StringProvider>(() => new StringProvider(new Application.Templates.TemplateOptions(), testCulture));
            });
        }

        protected override void OnProviderRegistered()
        {
            if (testCulture.Name == "zh-CN")
            {
                WZFiles.DIRECTORY += "-zh-CN";
            }
        }

        protected override void RunAfterTest()
        {
            WZFiles.DIRECTORY = WZFiles.getWzDirectory();
        }

        [Test]
        public void ItemTests()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = ProviderFactory.GetProvider<StringProvider>().GetSubProvider(StringCategory.Item);

            var oldList = oldProvider.GetAllItem().OrderBy(x => x.Id).ToList();
            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, id " + item.Id);
                else
                    Assert.That(newData.Name, Is.EqualTo(item.Name), "id " + item.Id);
            }
        }

        [Test]
        public void MapTests()
        {
            var oldProvider = new WzStringProvider();


            var oldList = oldProvider.GetAllMap().OrderBy(x => x.Id).ToList();

            var newProvider = ProviderFactory.GetProvider<StringProvider>().GetSubProvider(StringCategory.Map);
            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringMapTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, mapid " + item.Id);
                else
                {
                    Assert.That(newData.MapName, Is.EqualTo(item.PlaceName), "mapid " + item.Id);
                    Assert.That(newData.StreetName, Is.EqualTo(item.StreetName), "mapid " + item.Id);
                }
            }
        }

        [Test]
        public void NpcTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = ProviderFactory.GetProvider<StringProvider>().GetSubProvider(Application.Templates.String.StringCategory.Npc);

            var oldList = oldProvider.GetAllNpcList().OrderBy(x => x.Id).ToList();

            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringNpcTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, npcId " + item.Id);
                else
                {
                    Assert.That(newData.Name, Is.EqualTo(item.Name), "npcId " + item.Id);
                    Assert.That(newData.DefaultTalk, Is.EqualTo(item.DefaultTalk), "npcId " + item.Id);
                }
            }

        }

        [Test]
        public void MobTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = ProviderFactory.GetProvider<StringProvider>().GetSubProvider(Application.Templates.String.StringCategory.Mob);

            var oldList = oldProvider.GetAllMonster().OrderBy(x => x.Id).ToList();

            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, mobId " + item.Id);
                else
                {
                    Assert.That(newData.Name, Is.EqualTo(item.Name), "mobId " + item.Id);
                }
            }

        }
        [Test]
        public void SkillTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = ProviderFactory.GetProvider<StringProvider>().GetSubProvider(Application.Templates.String.StringCategory.Skill);

            var oldList = oldProvider.GetAllSkillList().OrderBy(x => x.Id).ToList();

            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, skillId " + item.Id);
                else
                {
                    Assert.That(newData.Name, Is.EqualTo(item.Name), "skillId " + item.Id);
                }
            }
        }
    }
}

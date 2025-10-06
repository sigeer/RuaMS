using Application.Resources;
using Application.Shared.Models;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using System.Globalization;

namespace ServiceTest.Infrastructure.WZ
{
    internal class StringTests
    {
        JsonSerializerSettings options;
        CultureInfo testCulture = CultureInfo.GetCultureInfo("en-US");
        public StringTests()
        {
            options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                Formatting = Formatting.Indented
            };

            ProviderFactory.Initilaize(o =>
            {
                o.RegisterProvider(new StringProvider(new Application.Templates.TemplateOptions(), testCulture));
            });
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }
        [Test]
        public void ItemTests()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions(), testCulture).GetSubProvider(StringCategory.Item);

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

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions(), testCulture).GetSubProvider(StringCategory.Map);
            foreach (var item in oldList)
            {
                var newData = newProvider.GetRequiredItem<StringMapTemplate>(item.Id);
                if (newData == null)
                    Assert.Fail("newData == null, mapid " + item.Id);
                else
                    Assert.That(newData.MapName, Is.EqualTo(item.PlaceName), "mapid " + item.Id);
            }
        }

        [Test]
        public void NpcTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions(), testCulture);

            var oldList = oldProvider.GetAllNpcList().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Npc).LoadAll().OrderBy(x => x.TemplateId).OfType<StringNpcTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new NpcObjectName(x.TemplateId, x.Name, x.DefaultTalk))), Is.EqualTo(ToJson(oldList)));
        }

        [Test]
        public void MobTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions(), testCulture);

            var oldList = oldProvider.GetAllMonster().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Mob).LoadAll().OrderBy(x => x.TemplateId).OfType<StringTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new ObjectName(x.TemplateId, x.Name))), Is.EqualTo(ToJson(oldList)));
        }
        [Test]
        public void SkillTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions(), testCulture);

            var oldList = oldProvider.GetAllSkillList().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Skill).LoadAll().OrderBy(x => x.TemplateId).OfType<StringTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new ObjectName(x.TemplateId, x.Name)).ToList()), Is.EqualTo(ToJson(oldList)));
        }
    }
}

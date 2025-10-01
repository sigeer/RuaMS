using Application.Resources;
using Application.Shared.Models;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;

namespace ServiceTest.Infrastructure.WZ
{
    internal class StringTests
    {
        JsonSerializerSettings options;

        public StringTests()
        {
            options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                Formatting = Formatting.Indented
            };
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }
        [Test]
        public void ItemTests()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions());

            var oldList = oldProvider.GetAllItem().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Item).LoadAll().OrderBy(x => x.TemplateId).OfType<StringTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new ObjectName(x.TemplateId, x.Name)).ToList()), Is.EqualTo(ToJson(oldList)));
        }

        [Test]
        public void MapTests()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions());

            var oldList = oldProvider.GetAllMap().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Map).LoadAll().OrderBy(x => x.TemplateId).OfType<StringMapTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new MapName(x.TemplateId, x.MapName, x.StreetName))), Is.EqualTo(ToJson(oldList)));
        }

        [Test]
        public void NpcTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions());

            var oldList = oldProvider.GetAllNpcList().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Npc).LoadAll().OrderBy(x => x.TemplateId).OfType<StringNpcTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new NpcObjectName(x.TemplateId, x.Name, x.DefaultTalk))), Is.EqualTo(ToJson(oldList)));
        }

        [Test]
        public void MobTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions());

            var oldList = oldProvider.GetAllMonster().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Mob).LoadAll().OrderBy(x => x.TemplateId).OfType<StringTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new ObjectName(x.TemplateId, x.Name))), Is.EqualTo(ToJson(oldList)));
        }
        [Test]
        public void SkillTest()
        {
            var oldProvider = new WzStringProvider();

            var newProvider = new StringProvider(new Application.Templates.TemplateOptions());

            var oldList = oldProvider.GetAllSkillList().OrderBy(x => x.Id).ToList();
            var newList = newProvider.GetSubProvider(Application.Templates.String.StringCategory.Skill).LoadAll().OrderBy(x => x.TemplateId).OfType<StringTemplate>().ToList();

            Assert.That(ToJson(newList.Select(x => new ObjectName(x.TemplateId, x.Name)).ToList()), Is.EqualTo(ToJson(oldList)));
        }
    }
}

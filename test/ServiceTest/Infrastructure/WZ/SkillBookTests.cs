using Application.Core.Channel.DataProviders;
using Application.Shared.Items;
using Application.Templates.Providers;
using Application.Templates.Quest;
using Application.Templates.XmlWzReader.Provider;
using client.inventory;
using Newtonsoft.Json;

namespace ServiceTest.Infrastructure.WZ
{
    public class SkillBookTests
    {
        [Test]
        public void LoadFromQuestTest()
        {
            ProviderFactory.Initilaize(o =>
            {
                o.RegisterProvider(new QuestProvider(new Application.Templates.TemplateOptions()));
            });
            var oldData = SkillbookInformationProvider.fetchSkillbooksFromQuests();
            var oldStr = JsonConvert.SerializeObject(oldData);

            var newData = SkillbookInformationProvider.FetchSkillbooksFromQuest();
            var provider = ProviderFactory.GetProvider<QuestProvider>();
            var newStr = JsonConvert.SerializeObject(newData);

            Assert.That(newStr, Is.EqualTo(oldStr));
        }
    }
}

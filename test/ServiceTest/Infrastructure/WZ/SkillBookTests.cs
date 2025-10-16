using Application.Core.Channel.DataProviders;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using ServiceTest.TestUtilities;
using System.Diagnostics;

namespace ServiceTest.Infrastructure.WZ
{
    public class SkillBookTests
    {
        [Test]
        public void LoadFromQuestTest()
        {
            Console.WriteLine($"TestVariable.WzPath: {TestVariable.WzPath}");

            ProviderFactory.Clear();
            ProviderFactory.Configure(o =>
            {
                o.DataDir = TestVariable.WzPath;

                o.RegisterProvider<QuestProvider>(() => new QuestProvider(new Application.Templates.TemplateOptions()));
            });
            var dataProvider = new SkillbookInformationProvider(null, null);
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var oldData = SkillbookInformationProvider.fetchSkillbooksFromQuests();
            sw.Stop();

            Console.WriteLine("old " + sw.Elapsed.TotalSeconds);
            var oldStr = JsonConvert.SerializeObject(oldData);

            sw.Restart();
            var newData = dataProvider.FetchSkillbooksFromQuest();
            sw.Stop();
            Console.WriteLine("new " + sw.Elapsed.TotalSeconds);

            var provider = ProviderFactory.GetProvider<QuestProvider>();
            var newStr = JsonConvert.SerializeObject(newData);

            Assert.That(newStr, Is.EqualTo(oldStr));
        }
    }
}

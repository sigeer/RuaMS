using Application.Core.Channel.DataProviders;
using Application.Templates;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using ServiceTest.TestUtilities;
using System.Diagnostics;

namespace ServiceTest.Infrastructure.WZ
{
    internal class SkillBookTests: WzTestBase
    {
        protected override void OnProviderRegistering()
        {
            _providerSource.TryRegisterProvider<QuestProvider>(o => new QuestProvider(o));
            ProviderSource.Instance = _providerSource;
        }
        [Test]
        public void LoadFromQuestTest()
        {
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

            var provider = _providerSource.GetProvider<QuestProvider>();
            var newStr = JsonConvert.SerializeObject(newData);

            Assert.That(newStr, Is.EqualTo(oldStr));
        }
    }
}

using Application.Core.Channel.DataProviders;
using Application.Templates.Reader.Img.Provider;
using Application.Templates.Quest;
using Application.Templates.Reader;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ServiceTest.Infrastructure.WZ
{
    [TestFixture("Duey")]
    [TestFixture("Xml")]
    internal class SkillBookTests(string readerType) : WzTestBase(readerType)
    {
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

            var provider = _providerSource.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
            var newStr = JsonConvert.SerializeObject(newData);

            Assert.That(newStr, Is.EqualTo(oldStr));
        }
    }
}

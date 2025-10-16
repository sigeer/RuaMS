using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using ServiceTest.TestUtilities;
using System.Globalization;

namespace ServiceTest.Games
{
    [SetUpFixture]
    public class GlobalSetup
    {
        public static LocalTestServer TestServer { get; private set; }
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            TestServer = new LocalTestServer();
            ProviderFactory.Clear();
            ProviderFactory.Configure(option =>
            {
                option.DataDir = TestVariable.WzPath;

                option.RegisterProvider(new MapProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider(new ReactorProvider(new Application.Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new QuestProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider(new EquipProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider(new ItemProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider(new MobSkillProvider(new Application.Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider(new EtcNpcLocationProvider(new Application.Templates.TemplateOptions()));

                option.RegisterKeydProvider("zh-CN", new StringProvider(new Application.Templates.TemplateOptions(), CultureInfo.GetCultureInfo("zh-CN")));
                option.RegisterKeydProvider("en-US", new StringProvider(new Application.Templates.TemplateOptions(), CultureInfo.GetCultureInfo("en-US")));
            });
            await TestServer.StartServer();
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            Console.WriteLine("Global teardown after all tests");
        }
    }
}

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

                option.RegisterProvider<MapProvider>(() => new MapProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider<ReactorProvider>(() => new ReactorProvider(new Application.Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider<QuestProvider>(() => new QuestProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider<EquipProvider>(() => new EquipProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider<ItemProvider>(() => new ItemProvider(new Application.Templates.TemplateOptions()));
                option.RegisterProvider<MobSkillProvider>(() => new MobSkillProvider(new Application.Templates.TemplateOptions() { UseCache = false }));
                option.RegisterProvider<EtcNpcLocationProvider>(() => new EtcNpcLocationProvider(new Application.Templates.TemplateOptions()));

                option.RegisterKeydProvider("zh-CN", () => new StringProvider(new Application.Templates.TemplateOptions(), CultureInfo.GetCultureInfo("zh-CN")));
                option.RegisterKeydProvider("en-US", () => new StringProvider(new Application.Templates.TemplateOptions(), CultureInfo.GetCultureInfo("en-US")));
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

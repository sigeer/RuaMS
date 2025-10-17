using Application.Templates.Providers;
using ServiceTest.TestUtilities;

namespace ServiceTest.Games
{
    [SetUpFixture]
    public class GameTestGlobal
    {
        public static LocalTestServer TestServer { get; private set; }
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            TestServer = new LocalTestServer();
            ProviderFactory.ConfigureWith(option =>
            {
                option.DataDir = TestVariable.WzPath;
            });
            await TestServer.StartServer();
        }
    }
}

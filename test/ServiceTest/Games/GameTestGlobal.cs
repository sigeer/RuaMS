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
            await TestServer.StartServer();
        }
    }
}

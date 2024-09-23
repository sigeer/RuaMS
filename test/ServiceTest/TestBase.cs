using Application.Core.Game;
using Application.Core.Managers;
using net.server;

namespace ServiceTest
{
    public class TestBase
    {
        public TestBase()
        {
            Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");
        }
        protected static IClient GenerateTestClient()
        {
            Server.getInstance().forceUpdateCurrentTime();
            var mockClient = Client.createMock();
            mockClient.setPlayer(CharacterManager.GetPlayerById(1));
            return mockClient;
        }
    }
}

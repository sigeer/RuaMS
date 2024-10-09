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
        protected static IClient GetOnlinedTestClient(int charId = 1)
        {
            Server.getInstance().forceUpdateCurrentTime();
            var mockClient = Client.createMock();
            mockClient.setPlayer(CharacterManager.GetPlayerById(charId));
            return mockClient;
        }
    }
}

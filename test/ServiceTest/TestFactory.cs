using client;
using Moq;
using net.packet;
using net.server;
using System.Diagnostics;

namespace ServiceTest
{
    public class TestFactory
    {
        public static Client GenerateTestClient()
        {
            Server.getInstance().forceUpdateCurrentTime();

            var mockObj = new Mock<Client>(null, null, null);
            mockObj.Setup(x => x.sendPacket(It.IsAny<Packet>())).Callback<Packet>((bytes) => Console.WriteLine($"client.sendPacket({bytes}), {new StackTrace().ToString()}"));
            mockObj.Setup(x => x.getPlayer()).Returns(() => Character.GetDefaultCharacter(0, 0));
            return mockObj.Object;
        }
    }
}

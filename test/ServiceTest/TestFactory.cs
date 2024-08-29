using Application.Core.Game;
using Application.Core.Managers;
using Moq;
using net.packet;
using net.server;
using System.Diagnostics;

namespace ServiceTest
{
    public class TestFactory
    {
        public static IClient GenerateTestClient()
        {
            Server.getInstance().forceUpdateCurrentTime();

            var mockObj = new Mock<IClient>(() => new OfflineClient());
            mockObj.Setup(x => x.sendPacket(It.IsAny<Packet>())).Callback<Packet>((bytes) => Console.WriteLine($"client.sendPacket({bytes}), {new StackTrace().ToString()}"));
            mockObj.Setup(x => x.getPlayer()).Returns(() => CharacterManager.NewPlayer(0, 0));
            return mockObj.Object;
        }
    }
}

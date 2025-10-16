using ServiceTest.Games;

namespace ServiceTest.Games.Services
{
    public class ChannelServiceTest
    {
        [Test]
        public void RequestReactorDropTest()
        {
            var data = GlobalSetup.TestServer.GetChannel(1).Service.RequestAllReactorDrops();
            Assert.That(data.Count > 0);
        }
    }
}

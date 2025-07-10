using System.Threading.Tasks;

namespace ServiceTest.ChannelTests
{
    public class ChannelServiceTest : TestBase
    {
        [Test]
        public async Task RequestReactorDropTest()
        {
            await LoadServer();
            var data = GetChannel(1).Service.RequestAllReactorDrops();
            Assert.That(data.Count > 0);
        }
    }
}

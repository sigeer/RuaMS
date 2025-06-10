using Application.Core.Channel;
using Application.Core.Game.TheWorld;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceTest.ChannelTests
{
    public class ChannelServiceTest : TestBase
    {
        WorldChannel _channel;
        public ChannelServiceTest()
        {
            var scope = _sp.CreateScope();
            var transport = _sp.GetRequiredService<IChannelServerTransport>();
            var config = new Application.Shared.Servers.WorldChannelConfig("TEST")
            {
                Port = 7575
            };
            _channel = new Application.Core.Channel.WorldChannel(scope, config);
        }
        [Test]
        public void RequestReactorDropTest()
        {
            var data = _channel.Service.RequestAllReactorDrops();
            Assert.That(data.Count > 0);
        }
    }
}

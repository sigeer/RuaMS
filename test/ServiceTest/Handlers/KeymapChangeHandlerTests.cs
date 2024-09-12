using DotNetty.Buffers;
using net.packet;
using net.server.channel.handlers;

namespace ServiceTest.Handlers
{
    public class KeymapChangeHandlerTests
    {
        [TestCase("135,0,1,0,0,0,0,0,0,0")]
        [Test]
        public void HandleTest(string bytesString)
        {
            var bytes = bytesString.Split(',').Select(byte.Parse).ToArray();
            var reader = new ByteBufInPacket(Unpooled.WrappedBuffer(bytes));
            reader.readShort(); // get packet handler
            new KeymapChangeHandler().HandlePacket(reader, TestFactory.GenerateTestClient());
            Assert.Pass();
        }

        //[TestCase("179,188,185,188,49,145,186,204,68,212,156,152,191,93,12,124,6,124,227,245,73,28,76,34,6,185,60,171")]
        //[Test]
        //public async Task FromNetHandleTest(string bytesString)
        //{
        //    var bytes = bytesString.Split(',').Select(byte.Parse).ToArray();
        //    var client = new MockupClient();
        //    await client.Initialize();
        //    await client.Send(bytes);
        //    await Task.Delay(TimeSpan.FromMinutes(1));
        //    Assert.Pass();
        //}
    }
}

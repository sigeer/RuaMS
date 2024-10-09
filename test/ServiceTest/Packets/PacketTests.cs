
using DotNetty.Buffers;
using net.packet;

namespace ServiceTest.Packets
{
    public class PacketTests
    {
        [TestCase("中文测试", ExpectedResult = 13)]
        [TestCase("abc", ExpectedResult = 13)]
        [Test]
        public int WriteFixedString_Test(string str)
        {
            var writer = new ByteBufOutPacket();
            writer.writeFixedString(str);
            var packet = writer.getBytes();
            return packet.Length;
        }

        [TestCase("中文测试")]
        [TestCase("abc")]
        [Test]
        public void Read_WriteString_Test(string str)
        {
            var writer = new ByteBufOutPacket();
            writer.writeString(str);
            var writeBytes = writer.getBytes();

            var reader = new ByteBufInPacket(Unpooled.WrappedBuffer(writeBytes));
            var readStr = reader.readString();
            Assert.That(readStr, Is.EqualTo(str));
        }
    }
}

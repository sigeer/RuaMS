
using net.packet;

namespace ServiceTest.Packets
{
    public class PacketWriterTests
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
    }
}


using net.packet;

namespace ServiceTest.Packets
{
    public class PacketWriterTests
    {
        [TestCase("中文测试", ExpectedResult = 13)]
        [TestCase("abc", ExpectedResult = 13)]
        [Test]
        public int WriteString_Test(string str)
        {
            var writer = new ByteBufOutPacket();
            writer.writeString(str);
            var packet = writer.getBytes();
            return packet.Length;
        }
    }
}

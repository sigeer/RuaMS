
using Application.Shared.Net;
using DotNetty.Buffers;
using System.Text;

namespace ServiceTest.Packets
{
    public class PacketTests
    {
        public PacketTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

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

        [TestCase("长文本测试长文本测试长文本测试长文本测试")]
        [TestCase("中文测试")]
        [TestCase("abc")]
        [TestCase("abcdef  ")]
        [TestCase("  abcdef")]
        [Test]
        public void WriteStringTest(string str)
        {
            var oldWriter = new ByteBufOutPacket();
            oldWriter.writeStringOld(str);
            var oldBytes = oldWriter.getBytes();

            var newWriter = new ByteBufOutPacket();
            newWriter.WriteString(str);
            var newBytes = oldWriter.getBytes();

            Assert.That(oldBytes.SequenceEqual(newBytes));
        }

        [TestCase("长文本测试长文本测试长文本测试长文本测试")]
        [TestCase("中文测试")]
        [TestCase("abc")]
        [TestCase("abcdef  ")]
        [TestCase("  abcdef")]
        [Test]
        public void WriteFixStringTest(string str)
        {
            var oldWriter = new ByteBufOutPacket();
            oldWriter.writeFixedStringOld(str);
            var oldBytes = oldWriter.getBytes();

            var newWriter = new ByteBufOutPacket();
            newWriter.WriteFixedString(str);
            var newBytes = oldWriter.getBytes();

            Assert.That(oldBytes.SequenceEqual(newBytes));
        }
    }
}

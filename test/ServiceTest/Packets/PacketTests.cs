
using Application.Shared.Net;
using System.Text;

namespace ServiceTest.Packets
{
    public class PacketTests
    {
        public PacketTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        [TestCase("长文本测试长文本测试长文本测试长文本测试")]
        [TestCase("中文测试")]
        [TestCase("abc")]
        [TestCase("abcdef  ")]
        [TestCase("  abcdef")]
        [Test]
        public void WriteFixStringTest(string str)
        {
            var writer = new ByteBufOutPacket();
            writer.writeFixedString(str);
            var writeBytes = writer.getBytes();

            Assert.That(writeBytes.Length, Is.EqualTo(13));
        }

        [TestCase("长文本测试长文本测试长文本测试长文本测试")]
        [TestCase("中文测试")]
        [TestCase("abc")]
        [TestCase("abcdef  ")]
        [TestCase("  abcdef")]
        [Test]
        public void ReadStringTest(string str)
        {
            var writer = new ByteBufOutPacket();
            writer.writeString(str);
            var writeBytes = writer.getBytes();

            var outPacket = new ByteBufInPacket(writeBytes);
            var oldStr = outPacket.readStringOld();

            Assert.That(oldStr, Is.EqualTo(str));

            var readerNew = new ByteBufInPacket(writeBytes);
            var newStr = readerNew.readString();
            Assert.That(newStr, Is.EqualTo(str));

        }
    }
}

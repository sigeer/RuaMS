using Application.Utility;

namespace ServiceTest.Infrastructure
{
    internal class HexToolTests
    {
        [Test]
        [TestCase("00 01", ExpectedResult = "0, 1")]
        [TestCase("01 00", ExpectedResult = "1, 0")]
        [TestCase("01 10 7F FF", ExpectedResult = "1, 16, 127, -1")]
        public string ToBytesTest(string str)
        {
            var d = HexTool.toBytes(str).Cast<sbyte>().ToList();
            return string.Join(", ", d);
        }
    }
}

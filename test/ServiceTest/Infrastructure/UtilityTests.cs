using Application.Utility;
using Application.Utility.Compatible;
using Application.Utility.Extensions;

namespace ServiceTest.Infrastructure
{
    public class UtilityTests
    {
        [Test]
        public void CollectionShuffleTest()
        {
            var list = Enumerable.Range(1, 10000).ToList();
            Assert.Throws<ArgumentException>(() =>
            {

                var count = 10000;
                while (count > 0)
                {
                    OldShuffle(list);
                    count--;
                }
            });

            Assert.DoesNotThrow(() => Collections.shuffle(list));
        }


        public static void OldShuffle<TModel>(List<TModel> list)
        {
            var comparedList = new int[] { -1, 0, 1 };
            list.Sort((o1, o2) => comparedList[Randomizer.rand(0, 2)]);
        }

        [Test]
        public void ReplaceFirstTest()
        {
            var testStr = "%s&apos;s Meetballs: %s";

            var final = testStr.replaceFirst("%s", "qq").replaceFirst("%s", "ww");
            Assert.That(final, Is.EqualTo("qq&apos;s Meetballs: ww"));
        }


        [TestCase("abc123", ExpectedResult = "abc, 123")]
        [TestCase("abc123d", ExpectedResult = "abc123d")]
        [TestCase("abc", ExpectedResult = "abc")]
        [TestCase("123", ExpectedResult = "__123")]
        [Test]
        public string SplitSuffixNumberTest(string input)
        {
            var final = input.SplitSuffixNumber();
            return string.Join(", ", final);
        }
    }
}

using Application.Core.Client.inventory;
using Application.Utility;
using Application.Utility.Compatible;
using Application.Utility.Extensions;
using DotNetty.Common.Utilities;

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

        [Test]
        public void GetSwapStepsTest()
        {
            List<string?> caseData = ["A", "B", "C", "D", "E", "F", "G", null, null, null];

            List<string?> caseFinal = caseData.OrderBy(x => Guid.NewGuid().ToString()).ToList();
            Console.WriteLine("Case: " + string.Join(",", caseFinal));
            var steps = InventorySorter.GetSwapSteps(caseData, caseFinal);

            var work = caseData.ToList();
            foreach (var (a, b) in steps)
            {
                Console.WriteLine($"Step: {a}, {b}");
                (work[a], work[b]) = (work[b], work[a]);
            }
            Console.WriteLine("Work: " + string.Join(",", work));
            Assert.That(caseFinal, Is.EqualTo(work));
        }
    }
}

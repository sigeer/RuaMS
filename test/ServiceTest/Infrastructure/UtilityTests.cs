using Application.Utility;
using Application.Utility.Compatible;

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
    }
}

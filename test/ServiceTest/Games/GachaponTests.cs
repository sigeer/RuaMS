using Application.Core.Channel.ServerData;
using Application.Core.tools.RandomUtils;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ServiceTest.Games
{
    public class GachaponTests : TestBase
    {
        GachaponManager _gachaponManager;

        public GachaponTests()
        {
            _gachaponManager = _sp.GetRequiredService<GachaponManager>();
        }

        [Test]
        public async Task GetLootInfoTest()
        {
            await LoadServer();
            var arr = _gachaponManager.GetLootInfo();
            Console.WriteLine(string.Join(Environment.NewLine, arr));
            Assert.That(arr.Length, Is.EqualTo(13));
        }

        [Test]
        public void GetItemTest()
        {
            Assert.That(_gachaponManager.GetItems(2, 2).Count > 0);
        }

        [Test]
        public void LotteryMachineTest()
        {
            List<LotteryMachinItem<int>> chanceList = [new LotteryMachinItem<int>(0, 90), new LotteryMachinItem<int>(1, 8), new LotteryMachinItem<int>(2, 2)];
            var machine = new LotteryMachine<int>(chanceList);
            int p1 = 0; int p2 = 0; int p3 = 0;
            for (int i = 0; i < 100000; i++)
            {
                var m = machine.GetRandomItem();
                if (m == 0)
                    p1++;
                if (m == 1)
                    p2++;
                if (m == 2)
                    p3++;
            }
            var r1 = p1 / 100000d;
            var r2 = p2 / 100000d;
            var r3 = p3 / 100000d;
            Console.WriteLine($"{r1} - {r2} - {r3}");
            Assert.That(Math.Round(r1, 2), Is.EqualTo(0.90));
            Assert.That(Math.Round(r2, 2), Is.EqualTo(0.08));
            Assert.That(Math.Round(r3, 2), Is.EqualTo(0.02));
        }
    }
}

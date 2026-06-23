using Application.Core.Channel.DataProviders;
using Application.Shared.Constants.Item;

namespace ServiceTest.Games.Gameplay
{
    internal class PlayerTests
    {
        [Test]
        public async Task GainMesoTest()
        {
            var chr = await GameTestGlobal.TestServer.GetPlayer()!;
            chr.MesoValue.set(0);

            await chr.GainMeso(1000);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(1000));

            var left = await chr.GainMeso(-10000);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(0));
            Assert.That(left, Is.EqualTo(-9000));

            chr.MesoValue.set(int.MaxValue - 100);
            left = await chr.GainMeso(101);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(int.MaxValue));
            Assert.That(left, Is.EqualTo(1));
        }

        [Test]
        public async Task GainItemTest()
        {
            var chr = await GameTestGlobal.TestServer.GetPlayer()!;
            var stackableItemId = 2000006;
            var stackableSlotMax = ItemInformationProvider.getInstance().getSlotMax(chr.Client, stackableItemId);

            await chr.GainItem(stackableItemId, -short.MaxValue);
            var freeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            var q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(0));

            await chr.GainItem(stackableItemId, 1);

            q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(1));
            var currentFreeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            Assert.That(freeSlot - 1, Is.EqualTo(currentFreeSlot));

            await chr.GainItem(stackableItemId, stackableSlotMax);

            q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(stackableSlotMax + 1));
            currentFreeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            Assert.That(freeSlot - 2, Is.EqualTo(currentFreeSlot));
        }
    }
}

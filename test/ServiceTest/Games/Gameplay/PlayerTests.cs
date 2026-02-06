using Application.Core.Channel.DataProviders;
using Application.Shared.Constants.Item;

namespace ServiceTest.Games.Gameplay
{
    internal class PlayerTests
    {
        [Test]
        public void GainMesoTest()
        {
            var chr = GameTestGlobal.TestServer.GetPlayer()!;
            chr.MesoValue.set(0);

            chr.GainMeso(1000);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(1000));

            var left = chr.GainMeso(-10000);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(0));
            Assert.That(left, Is.EqualTo(-9000));

            chr.MesoValue.set(int.MaxValue - 100);
            left = chr.GainMeso(101);
            Assert.That(chr.MesoValue.get(), Is.EqualTo(int.MaxValue));
            Assert.That(left, Is.EqualTo(1));
        }

        [Test]
        public void GainItemTest()
        {
            var chr = GameTestGlobal.TestServer.GetPlayer()!;
            var stackableItemId = 2000006;
            var stackableSlotMax = ItemInformationProvider.getInstance().getSlotMax(chr.Client, stackableItemId);

            chr.GainItem(stackableItemId, -short.MaxValue);
            var freeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            var q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(0));

            chr.GainItem(stackableItemId, 1);

            q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(1));
            var currentFreeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            Assert.That(freeSlot - 1, Is.EqualTo(currentFreeSlot));

            chr.GainItem(stackableItemId, stackableSlotMax);

            q = chr.getItemQuantity(stackableItemId);
            Assert.That(q, Is.EqualTo(stackableSlotMax + 1));
            currentFreeSlot = chr.Bag[ItemConstants.getInventoryType(stackableItemId)].getNumFreeSlot();
            Assert.That(freeSlot - 2, Is.EqualTo(currentFreeSlot));
        }
    }
}

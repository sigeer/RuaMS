using Application.Shared.Items;
using Application.Shared.Net;
using client.inventory;

namespace Application.Module.Maker.Channel.Net
{
    internal class MakerPacketCreator
    {
        // MAKER_RESULT packets thanks to Arnah (Vertisy)
        public static Packet makerResult(bool success, int itemMade, int itemCount, int mesos, List<ItemQuantity> itemsLost, int catalystID, List<int> INCBuffGems)
        {
            OutPacket p = OutPacket.create(SendOpcode.MAKER_RESULT);
            p.writeInt(success ? 0 : 1); // 0 = success, 1 = fail
            p.writeInt(1); // 1 or 2 doesn't matter, same methods
            p.writeBool(!success);
            if (success)
            {
                p.writeInt(itemMade);
                p.writeInt(itemCount);
            }
            p.writeInt(itemsLost.Count); // Loop
            foreach (var item in itemsLost)
            {
                p.writeInt(item.ItemId);
                p.writeInt(item.Quantity);
            }
            p.writeInt(INCBuffGems.Count);
            foreach (int gem in INCBuffGems)
            {
                p.writeInt(gem);
            }
            if (catalystID != -1)
            {
                p.writeByte(1); // stimulator
                p.writeInt(catalystID);
            }
            else
            {
                p.writeByte(0);
            }

            p.writeInt(mesos);
            return p;
        }

        public static Packet makerResultCrystal(int itemIdGained, int itemIdLost)
        {
            OutPacket p = OutPacket.create(SendOpcode.MAKER_RESULT);
            p.writeInt(0); // Always successful!
            p.writeInt(3); // NewMonster Crystal
            p.writeInt(itemIdGained);
            p.writeInt(itemIdLost);
            return p;
        }

        public static Packet makerResultDesynth(int itemId, int mesos, List<ItemQuantity> itemsGained)
        {
            OutPacket p = OutPacket.create(SendOpcode.MAKER_RESULT);
            p.writeInt(0); // Always successful!
            p.writeInt(4); // Mode Desynth
            p.writeInt(itemId); // Item desynthed
            p.writeInt(itemsGained.Count); // Loop of items gained, (int, int)
            foreach (var item in itemsGained)
            {
                p.writeInt(item.ItemId);
                p.writeInt(item.Quantity);
            }
            p.writeInt(mesos); // Mesos spent.
            return p;
        }

        public static Packet makerEnableActions()
        {
            OutPacket p = OutPacket.create(SendOpcode.MAKER_RESULT);
            p.writeInt(0); // Always successful!
            p.writeInt(0); // NewMonster Crystal
            p.writeInt(0);
            p.writeInt(0);
            return p;
        }
    }
}

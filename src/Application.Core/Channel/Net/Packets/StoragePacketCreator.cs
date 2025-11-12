using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Packets
{
    internal class StoragePacketCreator
    {
        public static Packet getStorage(int npcId, byte slots, List<Item> items, int meso)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0x16);
            p.writeInt(npcId);
            p.writeByte(slots);
            p.writeShort(0x7E);
            p.writeShort(0);
            p.writeInt(0);
            p.writeInt(meso);
            p.writeShort(0);
            p.writeByte(items.Count);
            foreach (Item item in items)
            {
                PacketCreator.addItemInfo(p, item, true);
            }
            p.writeShort(0);
            p.writeByte(0);
            return p;
        }

        /// <summary>
        /// 0x0A = Inv full
        /// 0x11 = Storage full
        /// 0x0B = You do not have enough mesos
        /// 0x0C = One-Of-A-Kind error
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Packet getStorageError(byte i)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(i);
            return p;
        }

        public static Packet mesoStorage(byte slots, int meso)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0x13);
            p.writeByte(slots);
            p.writeShort(2);
            p.writeShort(0);
            p.writeInt(0);
            p.writeInt(meso);
            return p;
        }

        public static Packet storeStorage(byte slots, InventoryType type, List<Item> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0xD);
            p.writeByte(slots);
            p.writeShort(type.getBitfieldEncoding());
            p.writeShort(0);
            p.writeInt(0);
            p.writeByte(items.Count());
            foreach (Item item in items)
            {
                PacketCreator.addItemInfo(p, item, true);
            }
            return p;
        }

        public static Packet takeOutStorage(byte slots, InventoryType type, List<Item> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0x9);
            p.writeByte(slots);
            p.writeShort(type.getBitfieldEncoding());
            p.writeShort(0);
            p.writeInt(0);
            p.writeByte(items.Count());
            foreach (Item item in items)
            {
                PacketCreator.addItemInfo(p, item, true);
            }
            return p;
        }

        public static Packet arrangeStorage(byte slots, List<Item> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0xF);
            p.writeByte(slots);
            p.writeByte(124);
            p.skip(10);
            p.writeByte(items.Count());
            foreach (Item item in items)
            {
                PacketCreator.addItemInfo(p, item, true);
            }
            p.writeByte(0);
            return p;
        }

    }
}

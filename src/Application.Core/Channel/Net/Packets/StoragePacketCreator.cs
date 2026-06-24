using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// void __cdecl CTrunkDlg::OnPacket(int a1)
    /// </summary>
    internal class StoragePacketCreator
    {
        public static Packet getStorage(int npcId, byte slots, List<Item> items, int meso)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0x16);
            p.writeInt(npcId);
            p.writeByte(slots);
            // 01111110 正好对应不同栏物品
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
            // 2 << 0
            p.writeShort(2);
            p.writeShort(0);
            p.writeInt(0);
            p.writeInt(meso);
            return p;
        }

        public static Packet storeStorage(byte slots, InventoryType type, List<Item> typedItems)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0xD);
            p.writeByte(slots);
            p.writeShort(type.getBitfieldEncoding());
            p.writeShort(0);
            p.writeInt(0);
            p.writeByte(typedItems.Count());
            foreach (Item item in typedItems)
            {
                PacketCreator.addItemInfo(p, item, true);
            }
            return p;
        }

        public static Packet takeOutStorage(byte slots, InventoryType type, List<Item> typedItems)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte(0x9);
            p.writeShort(type.getBitfieldEncoding());
            p.writeShort(0);
            p.writeInt(0);
            p.writeByte(typedItems.Count());
            foreach (Item item in typedItems)
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
            // 126(7E) - 2
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

        public static Packet OpenStorage(int npcId, int slots, Dictionary<InventoryType, List<Item>> items, int meso = 0)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte((int)StorageOperation.Load);
            p.writeInt(npcId);
            PacketCreator.AddSetGetItems(p, slots, items, meso);
            return p;
        }
        public static Packet EncodeStorage(StorageOperation op, int slots, Dictionary<InventoryType, List<Item>> items, int meso = 0)
        {
            OutPacket p = OutPacket.create(SendOpcode.STORAGE);
            p.writeByte((byte)op);
            PacketCreator.AddSetGetItems(p, slots, items, meso);
            return p;
        }
    }

    public enum StorageOperation : byte
    {
        /// <summary>
        /// 取出
        /// </summary>
        TakeOut = 0x9,
        /// <summary>
        /// 背包满 SP_853_PLEASE_CHECK_IF_YOUR_INVENTORY_IS_FULL_OR_NOT
        /// </summary>
        InvFull = 0xA,
        /// <summary>
        /// 费用 SP_5599_YOU_DO_NOT_HAVE_ENOUGH_MESOS
        /// </summary>
        FeeRequired = 0xB,
        /// <summary>
        /// 存在唯一道具 SP_866_ITEM_COULD_NOT_BE_RETRIEVED_R_NBECAUSE_THERE_WAS_AN_ITEM_THAT_R_NCOULD_ONLY_BE_A
        /// </summary>
        OneOfAKind = 0xC,
        /// <summary>
        /// 存入
        /// </summary>
        Store = 0xD,
        /// <summary>
        /// 排列 与 存入/取出金币在客户端的操作是一样的
        /// </summary>
        Arrange = 0xF,
        /// <summary>
        /// 仓库满了 SP_865_THE_STORAGE_IS_FULL
        /// </summary>
        StorageFull = 0x11,
        /// <summary>
        /// 存入/取出 金币
        /// </summary>
        MesoChange = 0x13,
        /// <summary>
        /// 打开仓库
        /// </summary>
        Load = 0x16,
    }
}

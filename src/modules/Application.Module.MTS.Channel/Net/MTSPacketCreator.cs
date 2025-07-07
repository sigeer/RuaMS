using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Shared.Net;
using server;
using tools;

namespace Application.Module.MTS.Channel.Net
{
    internal class MTSPacketCreator
    {
        public static Packet OpenMTS(IChannelClient c)
        {
            OutPacket p = OutPacket.create(SendOpcode.SET_ITC);

            PacketCreator.addCharacterInfo(p, c.OnlinedCharacter);

            p.writeString(c.AccountEntity.Name);
            p.writeBytes(new byte[]{ 0x88, 19, 0, 0,
                    7, 0, 0, 0,
                     0xF4, 1, 0, 0,
                     0x18, 0, 0, 0,
                     0xA8, 0, 0, 0,
                     0x70,  0xAA,  0xA7,  0xC5,
                     0x4E,  0xC1,  0xCA, 1});
            return p;
        }

        public static Packet sendMTS(List<MTSItemInfo> items, int tab, int type, int page, int pages)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x15); //operation
            p.writeInt(pages * 16); //testing, change to 10 if fails
            p.writeInt(items.Count); //number of items
            p.writeInt(tab);
            p.writeInt(type);
            p.writeInt(page);
            p.writeByte(1);
            p.writeByte(1);
            foreach (MTSItemInfo item in items)
            {
                PacketCreator.addItemInfo(p, item.getItem(), true);
                p.writeInt(item.getID()); //id
                p.writeInt(item.getTaxes()); //this + below = price
                p.writeInt(item.getPrice()); //price
                p.writeInt(0);
                p.writeLong(PacketCommon.getTime(item.getEndingDate()));
                p.writeString(item.getSeller()); //account name (what was nexon thinking?)
                p.writeString(item.getSeller()); //char name
                for (int j = 0; j < 28; j++)
                {
                    p.writeByte(0);
                }
            }
            p.writeByte(1);
            return p;
        }
        public static Packet showMTSCash(IPlayer chr)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION2);
            p.writeInt(chr.getCashShop().getCash(CashShop.NX_PREPAID));
            p.writeInt(chr.getCashShop().getCash(CashShop.MAPLE_POINT));
            return p;
        }

        public static Packet showMTSCash(int nxPrepaid, int maplePoint)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION2);
            p.writeInt(nxPrepaid);
            p.writeInt(maplePoint);
            return p;
        }

        public static Packet MTSWantedListingOver(int nx, int items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x3D);
            p.writeInt(nx);
            p.writeInt(items);
            return p;
        }

        public static Packet MTSConfirmSell()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x1D);
            return p;
        }

        public static Packet MTSConfirmBuy()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x33);
            return p;
        }

        public static Packet MTSFailBuy()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x34);
            p.writeByte(0x42);
            return p;
        }

        public static Packet MTSConfirmTransfer(int quantity, int pos)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x27);
            p.writeInt(quantity);
            p.writeInt(pos);
            return p;
        }

        public static Packet notYetSoldInv(List<MTSItemInfo> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x23);
            p.writeInt(items.Count);
            if (items.Count > 0)
            {
                foreach (MTSItemInfo item in items)
                {
                    PacketCreator.addItemInfo(p, item.getItem(), true);
                    p.writeInt(item.getID()); //id
                    p.writeInt(item.getTaxes()); //this + below = price
                    p.writeInt(item.getPrice()); //price
                    p.writeInt(0);
                    p.writeLong(PacketCommon.getTime(item.getEndingDate()));
                    p.writeString(item.getSeller()); //account name (what was nexon thinking?)
                    p.writeString(item.getSeller()); //char name
                    for (int i = 0; i < 28; i++)
                    {
                        p.writeByte(0);
                    }
                }
            }
            else
            {
                p.writeInt(0);
            }
            return p;
        }

        public static Packet transferInventory(List<MTSItemInfo> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(0x21);
            p.writeInt(items.Count);
            if (items.Count > 0)
            {
                foreach (MTSItemInfo item in items)
                {
                    PacketCreator.addItemInfo(p, item.getItem(), true);
                    p.writeInt(item.getID()); //id
                    p.writeInt(item.getTaxes()); //taxes
                    p.writeInt(item.getPrice()); //price
                    p.writeInt(0);
                    p.writeLong(PacketCommon.getTime(item.getEndingDate()));
                    p.writeString(item.getSeller()); //account name (what was nexon thinking?)
                    p.writeString(item.getSeller()); //char name
                    for (int i = 0; i < 28; i++)
                    {
                        p.writeByte(0);
                    }
                }
            }
            p.writeByte(0xD0 + items.Count);
            p.writeSBytes(new sbyte[] { -1, -1, -1, 0 });
            return p;
        }

    }
}

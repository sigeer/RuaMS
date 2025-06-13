namespace Application.Core.Channel.Net.Packets
{
    public class ChatRoomPacket
    {
        public static Packet addMessengerPlayer(string from, Dto.PlayerViewDto chr, int position, int channel)
        {
            OutPacket p = OutPacket.create(SendOpcode.MESSENGER);
            p.writeByte(0x00);
            p.writeByte(position);
            addCharLook(p, chr, true);
            p.writeString(from);
            p.writeByte(channel);
            p.writeByte(0x00);
            return p;
        }

        public static void addCharLook(OutPacket p, Dto.PlayerViewDto chr, bool mega)
        {
            p.writeByte(chr.Character.Gender);
            p.writeByte((int)chr.Character.Skincolor); // skin color
            p.writeInt(chr.Character.Face); // face
            p.writeBool(!mega);
            p.writeInt(chr.Character.Hair); // hair
            addCharEquips(p, chr.InventoryItems.ToArray());
        }

        private static void addCharEquips(OutPacket p, Dto.ItemDto[] equips)
        {
            Dictionary<short, int> myEquip = new();
            Dictionary<short, int> maskedEquip = new();
            int weaponItemId = 0;
            foreach (var item in equips.Where(x => x.InventoryType == -1))
            {
                short pos = (short)(item.Position * -1);
                if (pos < 100 && !myEquip.ContainsKey(pos))
                {
                    myEquip.AddOrUpdate(pos, item.Itemid);
                }
                else if (pos > 100 && pos != 111)
                {
                    // don't ask. o.o
                    pos -= 100;
                    if (myEquip.TryGetValue(pos, out var d))
                    {
                        maskedEquip.AddOrUpdate(pos, d);
                    }
                    myEquip.AddOrUpdate(pos, item.Itemid);
                }
                else if (myEquip.ContainsKey(pos))
                {
                    maskedEquip.AddOrUpdate(pos, item.Itemid);
                }

                if (item.Position == -111)
                    weaponItemId = item.Itemid;
            }
            foreach (var entry in myEquip)
            {
                p.writeByte(entry.Key);
                p.writeInt(entry.Value);
            }
            p.writeByte(0xFF);
            foreach (var entry in maskedEquip)
            {
                p.writeByte(entry.Key);
                p.writeInt(entry.Value);
            }
            p.writeByte(0xFF);
            p.writeInt(weaponItemId);
            for (int i = 0; i < 3; i++)
            {
                p.writeInt(0);
            }
        }

        public static Packet joinMessenger(int position)
        {
            OutPacket p = OutPacket.create(SendOpcode.MESSENGER);
            p.writeByte(0x01);
            p.writeByte(position);
            return p;
        }
    }
}

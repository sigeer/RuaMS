using Application.Core.Game.Life;

namespace Application.Core.Channel.Net.Packets
{
    public class PlayerNPCPacketCreator
    {
        public static Packet SpawnPlayerNPCController(PlayerNpc npc)
        {
            OutPacket p = OutPacket.create(SendOpcode.SPAWN_NPC_REQUEST_CONTROLLER);
            p.writeByte(1);
            p.writeInt(npc.getObjectId());
            p.writeInt(npc.NpcId);
            p.writeShort(npc.getPosition().X);
            p.writeShort(npc.Cy);
            p.writeByte(npc.Dir);
            p.writeShort(npc.Fh);
            p.writeShort(npc.Rx0);
            p.writeShort(npc.Rx1);
            p.writeByte(1);
            return p;
        }

        public static Packet GetPlayerNPC(PlayerNpc npc)
        {     // thanks to Arnah
            OutPacket p = OutPacket.create(SendOpcode.IMITATED_NPC_DATA);
            p.writeByte(1);
            p.writeInt(npc.NpcId);
            p.writeString(npc.Name);
            p.writeByte(npc.Gender);
            p.writeByte(npc.Skin);
            p.writeInt(npc.Face);
            p.writeByte(0);
            p.writeInt(npc.Hair);
            Dictionary<short, int> myEquip = new();
            Dictionary<short, int> maskedEquip = new();
            foreach (var item in npc.Equips)
            {
                short pos = (short)(item.EquipPos * -1);
                if (pos < 100 && !myEquip.ContainsKey(pos))
                {
                    myEquip.AddOrUpdate(pos, item.EquipId);
                }
                else if ((pos > 100 && pos != 111) || pos == -128)
                {
                    // don't ask. o.o
                    pos -= 100;
                    if (myEquip.TryGetValue(pos, out var d))
                    {
                        maskedEquip.AddOrUpdate(pos, d);
                    }
                    myEquip.AddOrUpdate(pos, item.EquipId);
                }
                else if (myEquip.ContainsKey(pos))
                {
                    maskedEquip.AddOrUpdate(pos, item.EquipId);
                }
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
            // -111是什么？
            var cWeapon = npc.Equips.FirstOrDefault(x => x.EquipPos == -111)?.EquipId ?? 0;
            p.writeInt(cWeapon);

            for (int i = 0; i < 3; i++)
            {
                p.writeInt(0);
            }
            return p;
        }

        public static Packet RemoveNPCController(int objId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SPAWN_NPC_REQUEST_CONTROLLER);
            p.writeByte(0);
            p.writeInt(objId);
            return p;
        }

        public static Packet RemovePlayerNPC(int oid)
        {
            OutPacket p = OutPacket.create(SendOpcode.IMITATED_NPC_DATA);
            p.writeByte(0x00);
            p.writeInt(oid);
            return p;
        }

    }
}

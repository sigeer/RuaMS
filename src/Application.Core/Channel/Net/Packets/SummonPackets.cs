using Application.Core.Game.Maps.AnimatedObjects;
using Application.Shared.Battle;
using Application.Shared.MapObjects.Summons;

namespace Application.Core.Channel.Net.Packets
{
    public class SummonPackets
    {
        public static Packet SpawnSummon(Summon summon, bool animated)
        {
            OutPacket p = OutPacket.create(SendOpcode.SPAWN_SPECIAL_MAPOBJECT);
            p.writeInt(summon.getOwner().getId());
            p.writeInt(summon.getObjectId());
            p.writeInt(summon.getSkill());
            p.writeByte(0x0A); //v83
            p.writeByte(summon.getSkillLevel());

            // CSummoned::Init
            p.writePos(summon.getPosition());
            p.writeByte(summon.getStance());    //bMoveAction & foothold, found thanks to Rien dev team
            p.writeShort(0);        // fh
            p.writeByte((byte)summon.MovementType); // 0 = don't move, 1 = follow (4th mage summons?), 2/4 = only tele follow, 3 = bird follow
            p.writeByte((byte)summon.AssistantType); // 0 and the summon can't attack - but puppets don't attack with 1 either ^.- 这里是否应该用

            p.writeBool(!animated);
            return p;
        }

        public static Packet RemoveSummon(Summon summon, SummonRemoveType animated)
        {
            OutPacket p = OutPacket.create(SendOpcode.REMOVE_SPECIAL_MAPOBJECT);
            p.writeInt(summon.getOwner().getId());
            p.writeInt(summon.getObjectId());
            p.writeByte((byte)animated); // ?
            return p;
        }

        public static Packet MoveSummon(int cid, int oid, Point startPos, InPacket movementPacket, int movementDataLength)
        {
            OutPacket p = OutPacket.create(SendOpcode.MOVE_SUMMON);
            p.writeInt(cid);
            p.writeInt(oid);
            p.writePos(startPos);
            PacketCommon.RebroadcastMovementList(p, movementPacket, movementDataLength);
            return p;
        }

        public static Packet SummonAttack(int cid, int summonOid, byte direction, List<SummonAttackEntry> allDamage)
        {
            OutPacket p = OutPacket.create(SendOpcode.SUMMON_ATTACK);
            //b2 00 29 f7 00 00 9a a3 04 00 c8 04 01 94 a3 04 00 06 ff 2b 00
            p.writeInt(cid);
            p.writeInt(summonOid);
            p.writeByte(0);     // char level
            p.writeByte(direction);
            p.writeByte(allDamage.Count);
            foreach (var attackEntry in allDamage)
            {
                p.writeInt(attackEntry.monsterOid); // oid
                p.writeByte(6); // who knows
                p.writeInt(attackEntry.damage); // damage
            }

            return p;
        }

        public static Packet SummonDamaged(int cid, int oid, int damage, sbyte attackIdx, int monsterIdFrom, sbyte dir)
        {
            OutPacket p = OutPacket.create(SendOpcode.DAMAGE_SUMMON);
            p.writeInt(cid);
            p.writeInt(oid);
            p.writeSByte(attackIdx);
            p.writeInt(damage);         // damage display doesn't seem to work...
            if (attackIdx > -2)
            {
                p.writeInt(monsterIdFrom);
                p.writeSByte(dir);
            }
            return p;
        }

        public static Packet SummonSkill(int cid, int summonOId, sbyte newStance)
        {
            OutPacket p = OutPacket.create(SendOpcode.SUMMON_SKILL);
            p.writeInt(cid);
            p.writeInt(summonOId);
            p.writeSByte(newStance);
            return p;
        }
    }
}

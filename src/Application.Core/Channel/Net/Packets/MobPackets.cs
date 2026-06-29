using Application.Core.Game.Life;

namespace Application.Core.Channel.Net.Packets
{
    public static class MobPackets
    {
        public static OutPacket SpecialEffectBySkill(int mobOId, int skillId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SpecialEffectBySkill);
            p.writeInt(mobOId);
            p.writeInt(skillId); // v3 = CInPacket::Decode4(a2); Skill = CSkillInfo::GetSkill(v3);
            return p;
        }

        public static Packet MobSpeak(int mobOId, int info, int node)
        {
            OutPacket p = OutPacket.create(SendOpcode.MobSpeak);
            p.writeInt(mobOId);
            p.writeInt(info);
            p.writeInt(node);
            return p;
        }

        public static Packet MobDamageMob(Monster mob, int attackIdx, int damage)
        {
            OutPacket p = OutPacket.create(SendOpcode.MobAttackedByMob);
            p.writeInt(mob.getObjectId()); 
            p.writeByte(attackIdx); // v2 = CInPacket::Decode1(a2); v3 = v2; CMobTemplate::GetAttackInfo(v3);
            p.writeInt(damage);     // v36 = CInPacket::Decode4(a2); CMob::ShowDamage(v42, v36, 0, 0, 0, v30, v31);
            p.writeInt(mob.getId());    // *v41 = CInPacket::Decode4(a2); CMobTemplate::GetMobTemplate(v41[0])
            p.writeByte(1); // v38 = CInPacket::Decode1(a2); bFlip
            return p;
        }
    }
}

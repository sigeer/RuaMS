using Application.Core.Game.Life;
using server.life;
using tools;

namespace Application.Core.Game.Maps.Mists
{
    public class MobMist : Mist
    {
        private Monster mob;
        private MobSkill skill;
        public MobMist(Rectangle mistPosition, Monster mob, MobSkill skill) : base(mob.getMap(), mistPosition, skill.getX() * 100)
        {
            this.mob = mob;
            this.skill = skill;
            _isMobMist = true;
            _isPoisonMist = true;
            _isRecoveryMist = false;
        }


        public Monster getMobOwner()
        {
            return mob;
        }

        public override Packet makeSpawnData()
        {
            return PacketCreator.spawnMobMist(getObjectId(), mob.getId(), skill.getId(), this);
        }

        public override Packet makeFakeSpawnData(int level)
        {
            return PacketCreator.spawnMobMist(getObjectId(), mob.getId(), skill.getId(), this);
        }
    }
}

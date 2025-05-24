using Application.Core.Game.Skills;
using server;
using tools;

namespace Application.Core.Game.Maps.Mists
{
    public class PlayerMist : Mist
    {
        IPlayer owner;
        StatEffect source;

        public PlayerMist(Rectangle mistPosition, IPlayer owner, StatEffect source) : base(mistPosition, 8)
        {
            this.owner = owner;
            this.source = source;
            _isMobMist = false;
            _isRecoveryMist = false;
            _isPoisonMist = false;
            switch (source.getSourceId())
            {
                case Evan.RECOVERY_AURA:
                    _isRecoveryMist = true;
                    break;

                case Shadower.SMOKE_SCREEN: // Smoke Screen
                    _isPoisonMist = false;
                    break;

                case FPMage.POISON_MIST: // FP mist
                case BlazeWizard.FLAME_GEAR: // Flame Gear
                case NightWalker.POISON_BOMB: // Poison Bomb
                    _isPoisonMist = true;
                    break;
            }
        }

        public Skill getSourceSkill()
        {
            return SkillFactory.getSkill(source.getSourceId());
        }

        public IPlayer getOwner()
        {
            return owner;
        }

        public override Packet makeSpawnData()
        {
            var sourceSkill = getSourceSkill();
            return PacketCreator.spawnMist(getObjectId(), owner.getId(), sourceSkill.getId(), owner.getSkillLevel(sourceSkill), this);
        }

        public override Packet makeFakeSpawnData(int level)
        {
            return PacketCreator.spawnMist(getObjectId(), owner.getId(), getSourceSkill().getId(), level, this);
        }

        public bool makeChanceResult()
        {
            return source.makeChanceResult();
        }
    }
}

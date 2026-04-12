using Application.Core.Game.Skills;
using Application.Utility.Tickables;
using server;

namespace Application.Core.Game.Players.Tickables
{
    public sealed class EffectBehold : BuffStatValueHolder
    {
        public EffectBehold(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, effect, startTime, expiredAt, value)
        {
            var now = chr.getChannelServer().Node.getCurrentTime();

            Skill bHealing = SkillFactory.GetSkillTrust(DarkKnight.AURA_OF_BEHOLDER);
            int bHealingLvl = _chr.getSkillLevel(bHealing);
            if (bHealingLvl > 0)
            {
                _healEffect = bHealing.getEffect(bHealingLvl);

                auraPeriod = _healEffect.getX() * 1000;
                auraNext = now + auraPeriod;
            }

            Skill bBuff = SkillFactory.GetSkillTrust(DarkKnight.HEX_OF_BEHOLDER);
            int bHexLevel = _chr.getSkillLevel(bBuff);
            if (bHexLevel > 0)
            {
                _hexEffect = bBuff.getEffect(bHexLevel);

                hexPeriod = _hexEffect.getX() * 1000;
                hexNext = now + hexPeriod;
            }
        }

        long auraNext;
        long auraPeriod;

        long hexNext;
        long hexPeriod;

        StatEffect? _healEffect;
        StatEffect? _hexEffect;
        public override void OnTick(long now)
        {
            if (this.IsAvailable())
            {
                if (ExpiredAt <= now)
                {
                    Status = TickableStatus.Remove;
                    return;
                }

                if (auraNext <= now)
                {
                    _healEffect?.applyTo(_chr);

                    auraNext = now + auraPeriod;
                }

                if (hexNext <= now)
                {
                    _hexEffect?.applyTo(_chr);

                    hexNext = now + hexPeriod;
                }
            }
        }
    }
}

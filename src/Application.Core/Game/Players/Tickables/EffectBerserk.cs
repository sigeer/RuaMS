using Application.Core.Game.Skills;
using server;
using tools;

namespace Application.Core.Game.Players.Tickables
{
    internal sealed class EffectBerserk : BuffStatValueHolder
    {
        public EffectBerserk(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, effect, startTime, expiredAt, value)
        {
            Period = 5_000;
            Next = chr.getChannelServer().Node.getCurrentTime() + 3_000;
        }

        public override void OnTick(long now)
        {
            if (!Disabled)
            {
                if (ExpiredAt <= now)
                {
                    Disabled = true;
                    return;
                }

                if (Next <= now)
                {
                    if (_chr.JobModel == Job.DARKKNIGHT)
                    {
                        Skill BerserkX = SkillFactory.GetSkillTrust(DarkKnight.BERSERK);
                        int skilllevel = _chr.getSkillLevel(BerserkX);
                        if (skilllevel > 0)
                        {
                            var buffEffect = BerserkX.getEffect(skilllevel);
                            var berserk = (_chr.HP * 100 / _chr.ActualMaxHP) < buffEffect.getX();

                            _chr.sendPacket(PacketCreator.showOwnBerserk(buffEffect.SkillLevel, berserk));
                            if (!_chr.isHidden())
                            {
                                _chr.MapModel.broadcastMessage(_chr, PacketCreator.showBerserk(_chr.Id, buffEffect.SkillLevel, berserk), false);
                            }
                            else
                            {
                                _chr.MapModel.broadcastGMMessage(_chr, PacketCreator.showBerserk(_chr.Id, buffEffect.SkillLevel, berserk), false);
                            }
                            Next = now + Period;
                        }
                    }
                }
            }
        }
    }
}

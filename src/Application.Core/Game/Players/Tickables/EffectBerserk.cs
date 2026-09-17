using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Skills;
using Application.Core.Server;
using tools;

namespace Application.Core.Game.Players.Tickables
{
    public sealed class EffectBerserk : EffectBuff
    {
        public EffectBerserk(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
            Period = 5_000;
            Next = chr.getChannelServer().Node.getCurrentTime() + 3_000;
        }

        protected override async Task Process(long now)
        {
            if (_chr.JobModel == Job.DARKKNIGHT)
            {
                var berserk = (_chr.HP * 100 / _chr.ActualMaxHP) < Effect.getX() ? (byte)1 : (byte)0;

                await _chr.SendPacket(EffectPacket.SkillEffect(Effect.getSourceId(), Effect.SkillLevel, _chr.Level, berserk));
                await _chr.BroadcastMap(EffectPacket.ForeignSkillEffect(_chr.Id, Effect.getSourceId(), Effect.SkillLevel, _chr.Level, berserk), _chr.Id);
            }
        }
    }
}

using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Utility.Tickables;

namespace Application.Core.Game.Players.Tickables
{
    public class EffectMapChair : EffectBuff
    {
        public EffectMapChair(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value) : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
            var now = chr.getChannelServer().Node.getCurrentTime();
            var p = Player.getChairTaskIntervalRate(chr.ActualMaxHP, chr.ActualMaxMP);
            Period = p.Rate;
            Next = now + p.Rate;
        }

        public override async Task OnTick(long now)
        {
            if (this.IsAvailable() && YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
            {
                if (ExpiredAt <= now)
                {
                    Status = TickableStatus.Remove;
                    return;
                }

                if (Next <= now)
                {
                    await _chr.ApplayChairBuff();
                    Next = now + Period;
                }

            }
        }
    }
}

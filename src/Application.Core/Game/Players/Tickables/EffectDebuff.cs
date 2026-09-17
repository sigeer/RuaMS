using Application.Core.Channel.Net.Packets;
using Application.Core.Server.life;

namespace Application.Core.Game.Players.Tickables
{
    public class EffectDebuff : EffectBase
    {
        public MobSkill Effect { get; }

        public EffectDebuff(Player chr, BuffStat buffStat, MobSkill effect, long startTime, long expiredAt, int value)
            : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
            Effect = effect;
        }

        byte _flag = 0;
        protected override async Task Process(long now)
        {
            if (_flag % 4 != 0)
            {
                _flag++;
                return;
            }

            var p = BuffPackets.GiveRemoteBuff(_chr.Id, this);
            if (p != null)
            {
                await _chr.BroadcastMap(p);
            }
            _flag = 1;
        }
    }
}

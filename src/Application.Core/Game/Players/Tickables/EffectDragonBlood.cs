using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Skills;
using Application.Core.Server;
using tools;

namespace Application.Core.Game.Players.Tickables
{
    public sealed class EffectDragonBlood : BuffStatValueHolder
    {
        public EffectDragonBlood(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, effect, startTime, expiredAt, value)
        {
            Period = 4_000;
            Next = chr.getChannelServer().Node.getCurrentTime() + Period;
        }

        protected override async Task Process(long now)
        {
            await _chr.UpdateStatsChunk(async () =>
            {
                if (await _chr.ChangeHP(-Effect.getX()))
                {
                    await _chr.SendPacket(EffectPacket.SkillSpecial(Effect.getSourceId()));
                    await _chr.BroadcastMap(EffectPacket.ForeignSkillSpecial(_chr.getId(), Effect.getSourceId()), _chr.Id);
                }
                else
                {
                    Status = Utility.Tickables.TickableStatus.Remove;
                    return;
                }
            });
        }
    }
}

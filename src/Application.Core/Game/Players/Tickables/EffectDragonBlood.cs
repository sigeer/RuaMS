using Application.Core.Game.Skills;
using server;
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
                    await _chr.SendPacket(PacketCreator.showOwnBuffEffect(Effect.getSourceId(), 5));
                    await _chr.BroadcastMap(PacketCreator.showBuffEffect(_chr.getId(), Effect.getSourceId(), 5), _chr.Id);
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

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

        protected override void Process(long now)
        {
            _chr.UpdateStatsChunk(() =>
            {
                if (_chr.ChangeHP(-effect.getX()))
                {
                    _chr.sendPacket(PacketCreator.showOwnBuffEffect(effect.getSourceId(), 5));
                    _chr.MapModel.broadcastMessage(_chr, PacketCreator.showBuffEffect(_chr.getId(), effect.getSourceId(), 5), false);
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

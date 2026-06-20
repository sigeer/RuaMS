using Application.Core.Game.Skills;
using server;
using tools;

namespace Application.Core.Game.Players.Tickables
{
    internal class EffectRecovery : BuffStatValueHolder
    {
        public EffectRecovery(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, effect, startTime, expiredAt, value)
        {
            Period = (YamlConfig.config.server.USE_ULTRA_RECOVERY) ? 2000 : 5000;
            Next = chr.getChannelServer().Node.getCurrentTime() + Period;
        }

        protected override async Task Process(long now)
        {
            await _chr.UpdateStatsChunk(async () =>
                {
                    await _chr.ChangeHP(value);
                });
            await _chr.SendPacket(PacketCreator.showOwnRecovery((sbyte)value));
            await _chr.BroadcastMap(PacketCreator.showRecovery(_chr.Id, (sbyte)value), _chr.Id);
        }
    }
}

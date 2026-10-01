using Application.Core.Game.Skills;
using Application.Core.Server;
using tools;

namespace Application.Core.Game.Players.Tickables
{
    public class EffectRecovery : EffectBuff
    {
        public EffectRecovery(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
            Period = (YamlConfig.config.server.USE_ULTRA_RECOVERY) ? 2000 : 5000;
            Next = chr.getChannelServer().Node.getCurrentTime() + Period;
        }

        protected override async Task Process(long now)
        {
            await _chr.UpdateStatsChunk(async () =>
                {
                    await _chr.ChangeHP(Value);
                });
            await _chr.SendPacket(PacketCreator.showOwnRecovery((sbyte)Value));
            await _chr.BroadcastMap(PacketCreator.showRecovery(_chr.Id, (sbyte)Value), _chr.Id);
        }
    }
}

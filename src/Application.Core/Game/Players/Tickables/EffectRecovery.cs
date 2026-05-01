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

        protected override void Process(long now)
        {
            _chr.UpdateStatsChunk(() =>
            {
                _chr.ChangeHP(value);
            });
            _chr.sendPacket(PacketCreator.showOwnRecovery((sbyte)value));
            _chr.MapModel.broadcastMessage(_chr, PacketCreator.showRecovery(_chr.Id, (sbyte)value), false);
        }
    }
}

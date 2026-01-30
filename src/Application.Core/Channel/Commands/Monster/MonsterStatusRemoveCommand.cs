using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using client.status;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class MonsterStatusRemoveCommand : IWorldChannelCommand
    {
        Monster _mob;
        MonsterStatusEffect _statueEffect;

        public MonsterStatusRemoveCommand(Monster mob, MonsterStatusEffect statueEffect)
        {
            _mob = mob;
            _statueEffect = statueEffect;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_mob.isAlive())
            {
                Packet packet = PacketCreator.cancelMonsterStatus(_mob.getObjectId(), _statueEffect.getStati());
                _mob.broadcastMonsterStatusMessage(packet);
            }

            var mobStatis = _mob.getStati();
            foreach (MonsterStatus stat in mobStatis.Keys)
            {
                mobStatis.Remove(stat);
            }

            _mob.setVenomMulti(0);
        }
    }
}

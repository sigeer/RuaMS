using Application.Core.Game.Life;
using client.status;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class MonsterBuffRemoveCommand : IWorldChannelCommand
    {
        Monster _mob;
        Dictionary<MonsterStatus, int> _stats;

        public MonsterBuffRemoveCommand(Monster mob, Dictionary<MonsterStatus, int> stats)
        {
            _mob = mob;
            _stats = stats;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_mob.isAlive())
            {
                Packet packet = PacketCreator.cancelMonsterStatus(_mob.getObjectId(), _stats);
                _mob.broadcastMonsterStatusMessage(packet);

                var mobSatis = _mob.getStati();
                foreach (MonsterStatus stat in _stats.Keys)
                {
                    mobSatis.Remove(stat);
                }
            }
        }
    }
}

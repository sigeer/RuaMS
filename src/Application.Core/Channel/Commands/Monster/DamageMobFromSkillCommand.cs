using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class DamageMobFromSkillCommand : IWorldChannelCommand
    {
        readonly IMap _map;
        readonly Monster _mob;
        Player _attacker;
        int _damage;

        public DamageMobFromSkillCommand(IMap map, Monster mob, Player attacker, int damage)
        {
            _map = map;
            _mob = mob;
            _attacker = attacker;
            _damage = damage;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.broadcastMessage(PacketCreator.damageMonster(_mob.getObjectId(), _damage), _mob.getPosition());
            _mob.DamageBy(_attacker, _damage, 0);
        }
    }
}

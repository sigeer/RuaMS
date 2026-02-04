using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;
using net.server.services.task.channel;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class MonsterApplyDamageCommand : IWorldChannelCommand
    {
        readonly Monster _monster;
        private Player chr;
        private MonsterStatusEffect status;
        private int dealDamage;
        private int type;
        private IMap map;

        public MonsterApplyDamageCommand(Monster monster, Player chr, MonsterStatusEffect status, int dealDamage, int type)
        {
            _monster = monster;
            this.chr = chr;
            this.status = status;
            this.dealDamage = dealDamage;
            this.type = type;
            this.map = chr.getMap();
        }

        public void Execute(ChannelCommandContext ctx)
        {
            int curHp = _monster.getHp();
            if (curHp <= 1)
            {
                MobStatusService service = map.getChannelServer().MobStatusService;
                service.interruptMobStatus(map.getId(), status);
                return;
            }

            int damage = dealDamage;
            if (damage >= curHp)
            {
                damage = curHp - 1;
                if (type == 1 || type == 2)
                {
                    MobStatusService service = map.getChannelServer().MobStatusService;
                    service.interruptMobStatus(map.getId(), status);
                }
            }
            if (damage > 0)
            {
                _monster.applyDamage(chr, damage, true, false);

                if (type == 1)
                {
                    map.broadcastMessage(PacketCreator.damageMonster(_monster.getObjectId(), damage), _monster.getPosition());
                }
                else if (type == 2)
                {
                    if (damage < dealDamage)
                    {    // ninja ambush (type 2) is already displaying DOT to the caster
                        map.broadcastMessage(PacketCreator.damageMonster(_monster.getObjectId(), damage), _monster.getPosition());
                    }
                }
            }
        }
    }
}

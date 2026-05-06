using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using server.life;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class MK_PrimeMinister : PartyQuestEventManager
    {
        public MK_PrimeMinister(WorldChannel cserv) : base(cserv, nameof(MK_PrimeMinister))
        {
            EventTime = 10 * 60;
            MinCount = 1;
            MaxCount = 3;
            MinLevel = 30;
            MaxLevel = 255;

            EntryMap = 106021600;
            EntryPortal = 1;
            ExitMap = 106021402;
            MinMap = 106021600;
            MaxMap = 106021600;
            RecruitMap = 106021402;

        }

        int mobId = 3300008;
        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == mobId)
            {
                eim.getMapInstance(EntryPortal).getPortal(1)?.setPortalState(true);

                eim.showClearEffect();
                eim.clearPQ();
            }
        }

        bool primeMinisterCheck(AbstractEventInstanceManager eim)
        {
            var map = eim.getMapInstance(EntryMap);

            foreach (var player in map.getAllPlayers())
            {
                if (player.getQuestStatus(2333) == 1 && player.getAbstractPlayerInteraction().getQuestProgressInt(2333, mobId) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            if (primeMinisterCheck(eim))
            {
                var weddinghall = eim.getMapInstance(EntryMap);
                weddinghall.getPortal(1)?.setPortalState(false);
                weddinghall.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), new Point(292, 143));
            }
            else
            {
                eim.Schedule(respawnStages, 10000);
            }
        }
    }
}

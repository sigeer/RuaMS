using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using server.life;
using System.Drawing;
using System.Threading.Tasks;

namespace Application.Plugin.Script.Events
{
    internal class MK_PrimeMinister : AbstractPartyQuestEventTemplate
    {
        public MK_PrimeMinister() : base(nameof(MK_PrimeMinister))
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
        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == mobId)
            {
                var map = await eim.getMapInstance(EntryPortal);
                map.getPortal(1)?.setPortalState(true);

                await eim.showClearEffect();
                await eim.clearPQ();
            }
        }

        async Task<bool> primeMinisterCheck(AbstractEventInstanceManager eim)
        {
            var map = await eim.getMapInstance(EntryMap);

            foreach (var player in map.getAllPlayers())
            {
                if (player.getQuestStatus(2333) == 1 && player.getAbstractPlayerInteraction().getQuestProgressInt(2333, mobId) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public override async Task respawnStages(AbstractEventInstanceManager eim)
        {
            if (await primeMinisterCheck(eim))
            {
                var weddinghall = await eim.getMapInstance(EntryMap);
                weddinghall.getPortal(1)?.setPortalState(false);
                await weddinghall.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), new Point(292, 143));
            }
            else
            {
                eim.Schedule(respawnStages, 10000);
            }
        }
    }
}

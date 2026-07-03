using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Ellin : AbstractPartyQuestEventTemplate
    {
        public PQ_Ellin() : base(nameof(PQ_Ellin))
        {
            MinCount = 4;
            MaxCount = 6;
            MinLevel = 44;
            MaxLevel = 55;
            EntryMap = 930000000;
            ExitMap = 930000800;
            RecruitMap = 300030100;
            ClearMap = 930000800;
            MinMap = 930000000;
            MaxMap = 930000800;

            EventTime = 30 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        int bossId = 9300182;

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == bossId)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();
            }
        }

        public override async Task OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            if (map.Id == 930000100)
            {
                await eim.showClearEffect(map.getId());
            }
        }
    }
}

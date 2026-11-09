using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.ElementalBattle
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 922020100;
        const int EXIT_MAP = 220050300;

        const int ELEMENTAL_1 = 9300086;
        const int ELEMENTAL_2 = 9300100;

        public Event() : base(ElementalBattleFeature.EventName)
        {
            MinCount = 2;
            MaxCount = 2;

            MinLevel = 100;
            MaxLevel = 255;

            RecruitMap = EXIT_MAP;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            MaxLobbys = 7;

            EventTime = 20 * 60;
        }

        /// <summary>
        /// JS 只允许法师系（job niche == 2）入队。
        /// </summary>
        public override List<Player> GetEligibleParty(Player leader)
        {
            return base.GetEligibleParty(leader)
                .Where(x => x.getJob().GetJobNiche() == 2)
                .ToList();
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("boss", 0);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var mobId = mob.getId();
            if (mobId != ELEMENTAL_1 && mobId != ELEMENTAL_2)
                return;

            var killed = eim.getIntProperty("boss");
            if (killed >= 1)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();
            }
            else
            {
                eim.setIntProperty("boss", killed + 1);
            }
        }

    }
}

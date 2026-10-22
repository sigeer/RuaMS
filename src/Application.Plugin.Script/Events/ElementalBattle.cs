using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 元素之战组队任务（Elemental Battle）。对应 JS scripts/event/ElementalBattle.js。
    /// 击杀两个元素 Boss 后通关。
    /// </summary>
    internal class ElementalBattle : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 922020100;
        const int EXIT_MAP = 220050300;

        const int ELEMENTAL_1 = 9300086;
        const int ELEMENTAL_2 = 9300100;

        public ElementalBattle() : base(nameof(ElementalBattle))
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

            EventTime = 20 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
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

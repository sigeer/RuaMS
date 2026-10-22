using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 暴力熊与心疤狮王远征（ScargaBattle）。对应 JS scripts/event/ScargaBattle.js。
    /// 两个 Boss 都击杀后通关。
    /// </summary>
    internal class Exped_Scarga : AbstractExpeditionEventTemplate
    {
        const int ENTRY_MAP = 551030200;
        const int EXIT_MAP = 551030100;

        const int SCARGA_BEAR = 9420544;
        const int SCARGA_LION = 9420549;

        public Exped_Scarga()
            : base(SCARGA_BEAR, nameof(Exped_Scarga), ExpeditionEntryType.SCARGA.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 100;
            MaxLevel = 255;

            RecruitMap = EXIT_MAP;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 60 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("defeatedBoss", 0);
            eim.setIntProperty("canJoin", 1);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var mobId = mob.getId();
            if (mobId != SCARGA_BEAR && mobId != SCARGA_LION)
                return;

            var killed = eim.getIntProperty("defeatedBoss");
            eim.setIntProperty("defeatedBoss", killed + 1);

            if (killed >= 1)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();
            }
        }
    }
}

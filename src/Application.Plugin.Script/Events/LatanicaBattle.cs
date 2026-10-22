using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 拉塔尼卡船长讨伐（Latanica Battle）。
    /// 对应 JS scripts/event/LatanicaBattle.js。
    /// </summary>
    internal class LatanicaBattle : AbstractPartyQuestEventTemplate
    {
        public LatanicaBattle() : base(nameof(LatanicaBattle))
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 541010060;
            EntryMap = 541010100;
            ExitMap = 541010110;
            ClearMap = 541010110;

            MinMap = 541010100;
            MaxMap = 541010100;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != 9420513)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }

    }
}

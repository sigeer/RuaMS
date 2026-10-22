using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 戴利战（Delli Battle）。
    /// 对应 JS scripts/event/DelliBattle.js。
    /// 通关条件是「撑满 6 分钟」：JS 的 scheduledTimeout 会清场、播放通关特效并 clearPQ。
    /// </summary>
    internal class DelliBattle : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 925010300;
        const int FRIENDLY_MOB = 9300162;

        public DelliBattle() : base(nameof(DelliBattle))
        {
            MinCount = 1;
            MaxCount = 2;
            MinLevel = 120;
            MaxLevel = 255;

            RecruitMap = 925010200;
            EntryMap = ENTRY_MAP;
            ExitMap = 925010200;
            ClearMap = 925010200;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 6 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            map.toggleDrops();
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            // JS scheduledTimeout：清场 → showClearEffect → clearPQ
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map != null)
            {
                await map.killAllMonstersNotFriendly();
            }

            await eim.showClearEffect();
            await ClearPQ(eim);
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // 被护送的戴利死亡 → 任务失败
            if (mob.getId() == FRIENDLY_MOB)
            {
                await End(eim, TerminationReason.Failure);
            }
        }
    }
}

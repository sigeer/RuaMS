using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.TDBattle
{
    /// <summary>
    /// 时空裂缝战斗（TD_Battle1 ~ TD_Battle5）。
    /// 对应 JS scripts/event/TD_Battle{1..5}.js —— 五份脚本除地图、时长与 Boss 外完全一致。
    /// 玩法：进入副本击杀指定 Boss 即通关。
    /// </summary>
    internal abstract class TDBattle : AbstractPartyQuestEventTemplate
    {
        /// <summary>本关卡的 Boss，击杀即通关。</summary>
        protected readonly int BossMobId;

        protected TDBattle(string name, int recruitMap, int bossMobId, int eventMinutes)
            : base(name)
        {
            BossMobId = bossMobId;

            MinCount = 2;
            MaxCount = 6;
            MinLevel = 70;
            MaxLevel = 255;

            RecruitMap = recruitMap;
            EntryMap = recruitMap + 1;
            ExitMap = recruitMap;
            ClearMap = recruitMap;

            MinMap = EntryMap;
            MaxMap = EntryMap;

            EventTime = eventMinutes * 60;
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
            if (mob.getId() != BossMobId)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }
    }
}

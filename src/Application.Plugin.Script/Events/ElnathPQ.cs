using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 冰峰雪域组队任务。
    /// 对应 JS scripts/event/ElnathPQ.js。
    /// 通关由阶段脚本调用 clearPQ 触发（JS 的 monsterKilled 为空函数）。
    /// </summary>
    internal class ElnathPQ : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 921100300;
        const int EXIT_MAP = 211040100;

        public ElnathPQ() : base(nameof(ElnathPQ))
        {
            MinCount = 1;
            MaxCount = 4;
            MinLevel = 80;
            MaxLevel = 255;

            RecruitMap = 211000001;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }
    }
}

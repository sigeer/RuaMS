using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{

    internal class SoloQuestEventTemplate : AbstractSoloEventTemplate
    {
        /// <summary>
        /// 任务 Id，同时也是事件注册键（q{QuestId}）。
        /// </summary>
        public int QuestId { get; }

        public SoloQuestEventTemplate(int quest, int eventTime, int entryMap, int exitMap, int minMap, int maxMap) : base($"q{quest}")
        {
            QuestId = quest;

            EventTime = eventTime;

            MaxLobbys = 1;

            EntryMap = entryMap;
            ExitMap = exitMap;

            MinMap = minMap;
            MaxMap = maxMap;
        }
    }
}

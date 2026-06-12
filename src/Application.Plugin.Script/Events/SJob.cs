using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{

    internal sealed class SoloQuestEventTemplate : AbstractSoloEventTemplate
    {
        int _questId;
        public SoloQuestEventTemplate(int quest, int eventTime, int entryMap, int exitMap, int minMap, int maxMap) : base($"q{quest}")
        {
            _questId = quest;

            EventTime = eventTime;

            MaxLobbys = 1;

            EntryMap = entryMap;
            ExitMap = exitMap;

            MinMap = minMap;
            MaxMap = maxMap;
        }
    }
}

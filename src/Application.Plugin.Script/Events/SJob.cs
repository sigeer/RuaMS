using Application.Core.Channel;
using Application.Core.Scripting.Events;
using Application.Shared.Quest;

namespace Application.Plugin.Script.Events
{

    internal class SoloQuestEventManager : SoloEventManager
    {
        int _questId;
        public SoloQuestEventManager(WorldChannel cserv, int quest, int eventTime, int entryMap, int exitMap, int minMap, int maxMap) : base(cserv, $"q{quest}")
        {
            _questId = quest;

            EventTime = eventTime;

            MaxLobbys = 1;

            EntryMap = entryMap;
            ExitMap = exitMap;

            MinMap = minMap;
            MaxMap = MaxMap;
        }
    }
}

using Application.Shared.Quest;

namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29001Ex : AbstractQuestRecordEx
    {
        [QuestRecordExKey("cmpcnt")]
        public int Count { get; set; }
        public MedalQuest29001Ex(string? rawContent) : base((short)MedalQuestId.Quest, rawContent)
        {
        }
    }
}

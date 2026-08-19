using Application.Shared.Quest;

namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29000Ex: AbstractQuestRecordEx
    {
        public MedalQuest29000Ex(string? rawContent) : base((short)ExQuestId.PartyQuest, rawContent)
        {
        }

        [QuestRecordExKey("scnt")]
        public int Scnt { get; set; }
    }
}

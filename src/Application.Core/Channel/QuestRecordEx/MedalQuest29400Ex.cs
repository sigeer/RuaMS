using Application.Shared.Quest;

namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29400Ex : AbstractQuestRecordEx
    {
        public MedalQuest29400Ex(string? rawContent) : base((short)MedalQuestId.VeteranHunter, rawContent)
        {
        }

        [QuestRecordExKey("mon")]
        public int Mon { get; set; }
        [QuestRecordExKey("mg")]
        public int Mg { get; set; }
    }
}

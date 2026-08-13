namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29000Ex: AbstractQuestRecordEx
    {
        public MedalQuest29000Ex(string rawContent) : base(29000, rawContent)
        {
        }

        [QuestRecordExKey("scnt")]
        public int Scnt { get; set; }
        [QuestRecordExKey("gaugePqS")]
        public int GaugePqS { get; set; }
        [QuestRecordExKey("perPqS")]
        public int PerPqS { get; set; }
    }
}

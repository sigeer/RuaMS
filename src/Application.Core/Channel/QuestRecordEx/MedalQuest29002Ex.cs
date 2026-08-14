namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29002Ex : AbstractQuestRecordEx
    {
        public MedalQuest29002Ex(string? rawContent) : base(29002, rawContent)
        {
        }

        [QuestRecordExKey("popG")]
        public int PopG { get; set; }
        [QuestRecordExKey("popS")]
        public int PopS { get; set; }
    }
}

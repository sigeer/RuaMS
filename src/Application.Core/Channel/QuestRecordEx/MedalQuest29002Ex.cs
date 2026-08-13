namespace Application.Core.Channel.QuestRecordEx
{
    public class MedalQuest29002Ex : AbstractQuestRecordEx
    {
        public MedalQuest29002Ex(string? rawContent) : base(29002, rawContent)
        {
        }

        /// <summary>
        /// 4479;"#jpopgap#"
        /// </summary>
        [QuestRecordExKey("popgap")]
        public int Popgap { get; set; }
        [QuestRecordExKey("popG")]
        public int PopG { get; set; }
    }
}

namespace Application.Templates.Quest
{
    public class PartyQuestTemplate
    {
        public PartyQuestRankCheckRule[] Less { get; set; }
        public PartyQuestRankCheckRule[] More { get; set; }
    }

    public class PartyQuestRankCheckRule
    {
        public int Minutes { get; set; }
        public int Try { get; set; }
        public int VR { get; set; }
        public int CR { get; set; }
    }
}

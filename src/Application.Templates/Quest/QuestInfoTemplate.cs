namespace Application.Templates.Quest
{
    public sealed class QuestInfoTemplate
    {
        public QuestInfoTemplate(int questId)
        {
            QuestId = questId;
            Name = string.Empty;
        }

        public int QuestId { get; set; }

        public string Name { get; set; }
        public string? Parent { get; set; }
        public int TimeLimit { get; set; }
        public int TimeLimit2 { get; set; }
        public bool AutoStart { get; set; }
        public bool AutoPreComplete { get; set; }
        public bool AutoComplete { get; set; }
        public int ViewMedalItem { get; set; }
    }
}

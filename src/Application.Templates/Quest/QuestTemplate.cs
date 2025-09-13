namespace Application.Templates.Quest
{
    public sealed class QuestTemplate : AbstractTemplate
    {
        public QuestTemplate(QuestInfoTemplate info) : base(info.QuestId)
        {
            Info = info;
        }

        public QuestInfoTemplate Info { get; set; }
        public QuestCheckTemplate? Check { get; set; }
        public QuestActTemplate? Act { get; set; }
    }
}

namespace Application.Core.EF.Entities.Quests
{
    public class QuestStatusEntityPair
    {
        public QuestStatusEntityPair(QuestStatusEntity questStatus, Questprogress[] progress, Medalmap[] medalmap)
        {
            QuestStatus = questStatus;
            Progress = progress;
            Medalmap = medalmap;
        }

        public QuestStatusEntity QuestStatus { get; set; }
        public Questprogress[] Progress { get; set; }
        public Medalmap[] Medalmap { get; set; }
    }
}

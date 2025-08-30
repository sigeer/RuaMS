namespace Application.Core.Login.Models
{
    public class QuestStatusModel
    {
        public int Id { get; set; }
        public int QuestId { get; set; }

        public int Status { get; set; }

        public int Time { get; set; }

        public long Expires { get; set; }

        public int Forfeited { get; set; }

        public int Completed { get; set; }

        public sbyte Info { get; set; }
        public int Characterid { get; set; }
        public QuestProgressModel[] Progress { get; set; }
        public MedalMapModel[] MedalMap { get; set; }
    }

    public class QuestProgressModel
    {
        public int ProgressId { get; set; }
        public string Progress { get; set; }
    }

    public class MedalMapModel
    {
        public int MapId { get; set; }
    }

    public class TimerQuestModel
    {
        public int QuestId { get; set; }
        public long ExpiredTime { get; set; }
    }
}

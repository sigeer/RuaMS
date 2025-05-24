namespace Application.Shared.Characters
{
    public class QuestStatusDto
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
        public QuestProgressDto[] Progress { get; set; }
        public MedalMapDto[] MedalMap { get; set; }
    }

    public class QuestProgressDto
    {
        public int ProgressId { get; set; }
        public string Progress { get; set; }
    }

    public class MedalMapDto
    {
        public int MapId { get; set; }
    }
}

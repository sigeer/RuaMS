namespace Application.EF.Entities;

public partial class QuestStatusEntity
{
    private QuestStatusEntity()
    {
    }

    public QuestStatusEntity(int characterid, int quest, int status, int time, long expires, int forfeited, int completed)
    {
        Characterid = characterid;
        Quest = quest;
        Status = status;
        Time = time;
        Expires = expires;
        Forfeited = forfeited;
        Completed = completed;
    }

    public int Queststatusid { get; set; }

    public int Characterid { get; set; }

    public int Quest { get; set; }

    public int Status { get; set; }

    public int Time { get; set; }

    public long Expires { get; set; }

    public int Forfeited { get; set; }

    public int Completed { get; set; }

    public sbyte Info { get; set; }
}

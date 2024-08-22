namespace Application.EF.Entities;

public partial class Questprogress
{
    private Questprogress()
    {
    }

    public Questprogress(int characterid, int queststatusid, int progressid, string progress)
    {
        Characterid = characterid;
        Queststatusid = queststatusid;
        Progressid = progressid;
        Progress = progress;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public int Queststatusid { get; set; }

    public int Progressid { get; set; }

    public string Progress { get; set; } = null!;
}

namespace Application.EF.Entities;

public partial class BosslogWeekly
{
    private BosslogWeekly()
    {
    }

    public BosslogWeekly(int characterId, string bosstype, DateTimeOffset time)
    {
        CharacterId = characterId;
        Bosstype = bosstype;
        Attempttime = time;
    }
    public int Id { get; set; }

    public int CharacterId { get; set; }

    public string Bosstype { get; set; } = null!;

    public DateTimeOffset Attempttime { get; set; }
}

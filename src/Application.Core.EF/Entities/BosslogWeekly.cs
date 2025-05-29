namespace Application.EF.Entities;

public partial class BosslogWeekly
{
    private BosslogWeekly()
    {
    }

    public BosslogWeekly(int characterId, string bosstype)
    {
        CharacterId = characterId;
        Bosstype = bosstype;
    }
    public int Id { get; set; }

    public int CharacterId { get; set; }

    public string Bosstype { get; set; } = null!;

    public DateTimeOffset Attempttime { get; set; }
}

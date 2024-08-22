namespace Application.EF.Entities;

public partial class Allianceguild
{
    public int Id { get; set; }

    public int AllianceId { get; set; }

    public int GuildId { get; set; }
}

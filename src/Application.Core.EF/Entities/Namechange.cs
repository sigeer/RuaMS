namespace Application.EF.Entities;

public partial class Namechange
{
    public int Id { get; set; }

    public int Characterid { get; set; }

    public string Old { get; set; } = null!;

    public string New { get; set; } = null!;

    public DateTimeOffset RequestTime { get; set; }

    public DateTimeOffset? CompletionTime { get; set; }
}

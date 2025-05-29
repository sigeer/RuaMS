namespace Application.EF.Entities;

public partial class Report
{
    public int Id { get; set; }

    public DateTimeOffset Reporttime { get; set; }

    public int Reporterid { get; set; }

    public int Victimid { get; set; }

    public sbyte Reason { get; set; }

    public string Chatlog { get; set; } = null!;

    public string Description { get; set; } = null!;
}

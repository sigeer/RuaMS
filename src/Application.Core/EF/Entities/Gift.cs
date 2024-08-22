namespace Application.EF.Entities;

public partial class Gift
{
    public int Id { get; set; }

    public int To { get; set; }

    public string From { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int Sn { get; set; }

    public int Ringid { get; set; }
}

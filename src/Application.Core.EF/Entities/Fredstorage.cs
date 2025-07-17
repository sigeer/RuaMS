namespace Application.EF.Entities;

public partial class Fredstorage
{
    public int Id { get; set; }

    public int Cid { get; set; }

    public int Daynotes { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}

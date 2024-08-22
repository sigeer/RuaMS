namespace Application.EF.Entities;

public partial class ServerQueue
{
    public int Id { get; set; }

    public int Accountid { get; set; }

    public int Characterid { get; set; }

    public sbyte Type { get; set; }

    public int Value { get; set; }

    public string Message { get; set; } = null!;

    public DateTimeOffset CreateTime { get; set; }
}

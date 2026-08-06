namespace Application.EF.Entities;

public partial class ReportEntity
{
    public int Id { get; set; }

    public DateTimeOffset ReportTime { get; set; } = DateTimeOffset.UtcNow;

    public int ReporterId { get; set; }

    public int VictimId { get; set; }

    public sbyte Reason { get; set; }

    public string Chatlog { get; set; } = null!;

    public string Description { get; set; } = null!;
    /// <summary>
    /// 已处理
    /// </summary>
    public bool Processed { get; set; }
}

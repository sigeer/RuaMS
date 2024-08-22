namespace Application.EF;

public partial class Nxcode
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Retriever { get; set; }

    public long Expiration { get; set; }
}

namespace Application.EF.Entities;

public partial class Hwidaccount
{
    public int AccountId { get; set; }

    public string Hwid { get; set; } = null!;

    public sbyte Relevance { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public Hwidaccount(string hWId, int accountId, DateTimeOffset expiresAt)
    {
        Hwid = hWId;
        AccountId = accountId;
        ExpiresAt = expiresAt;
    }

    private Hwidaccount()
    {
    }
}

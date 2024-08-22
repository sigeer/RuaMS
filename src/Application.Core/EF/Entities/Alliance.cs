namespace Application.EF.Entities;

public class DB_Alliance
{
    private DB_Alliance()
    {
    }

    public DB_Alliance(string name)
    {
        Name = name;
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string Notice { get; set; } = null!;

    public string Rank1 { get; set; } = null!;

    public string Rank2 { get; set; } = null!;

    public string Rank3 { get; set; } = null!;

    public string Rank4 { get; set; } = null!;

    public string Rank5 { get; set; } = null!;
}

namespace Application.EF.Entities;

public class AllianceEntity
{
    private AllianceEntity()
    {
    }

    public AllianceEntity(string name)
    {
        Name = name;
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; } = 2;

    public string Notice { get; set; } = "";

    public string Rank1 { get; set; } = null!;

    public string Rank2 { get; set; } = null!;

    public string Rank3 { get; set; } = null!;

    public string Rank4 { get; set; } = null!;

    public string Rank5 { get; set; } = null!;
}

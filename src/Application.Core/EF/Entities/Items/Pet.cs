namespace Application.EF.Entities;

public class PetEntity
{
    public PetEntity(int petid, string? name, int level, int closeness, int fullness, bool summoned, int flag)
    {
        Petid = petid;
        Name = name;
        Level = level;
        Closeness = closeness;
        Fullness = fullness;
        Summoned = summoned;
        Flag = flag;
    }

    protected PetEntity() { }
    public int Petid { get; set; }

    public string? Name { get; set; }

    public int Level { get; set; }

    public int Closeness { get; set; }

    public int Fullness { get; set; }

    public bool Summoned { get; set; }

    public int Flag { get; set; }

    public virtual ICollection<Petignore> Petignores { get; set; } = new List<Petignore>();
}

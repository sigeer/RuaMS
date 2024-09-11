namespace Application.EF.Entities;

public partial class PetEntity
{
    public int Petid { get; set; }

    public string? Name { get; set; }

    public int Level { get; set; }

    public int Closeness { get; set; }

    public int Fullness { get; set; }

    public bool Summoned { get; set; }

    public int Flag { get; set; }

    public virtual ICollection<Petignore> Petignores { get; set; } = new List<Petignore>();
}

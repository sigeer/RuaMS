namespace Application.EF.Entities;

public partial class Petignore
{
    protected Petignore() { }
    public Petignore(int petid, int itemid)
    {
        Petid = petid;
        Itemid = itemid;
    }

    public int Id { get; set; }

    public int Petid { get; set; }

    public int Itemid { get; set; }

    public virtual PetEntity Pet { get; set; } = null!;
}

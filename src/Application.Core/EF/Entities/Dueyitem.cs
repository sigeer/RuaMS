namespace Application.EF.Entities;

public partial class Dueyitem
{
    public int Id { get; set; }

    public int PackageId { get; set; }

    public int Inventoryitemid { get; set; }

    public virtual DueyPackageEntity Package { get; set; } = null!;
}

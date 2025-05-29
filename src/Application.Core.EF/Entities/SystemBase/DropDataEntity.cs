namespace Application.EF.Entities;

public partial class DropDataEntity
{
    public long Id { get; set; }

    public int Dropperid { get; set; }

    public int Itemid { get; set; }

    public int MinimumQuantity { get; set; }

    public int MaximumQuantity { get; set; }

    public int Questid { get; set; }

    public int Chance { get; set; }
}

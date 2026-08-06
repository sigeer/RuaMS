namespace Application.EF.Entities;


public partial class DropDataEntity
{
    private DropDataEntity() { }
    public DropDataEntity(int dropperid, int itemid, int minimumQuantity, int maximumQuantity, int questid, int chance)
    {
        Dropperid = dropperid;
        Itemid = itemid;
        MinimumQuantity = minimumQuantity;
        MaximumQuantity = maximumQuantity;
        Questid = questid;
        Chance = chance;
    }

    public long Id { get; set; }

    public int Dropperid { get; set; }

    public int Itemid { get; set; }

    public int MinimumQuantity { get; set; }

    public int MaximumQuantity { get; set; }

    public int Questid { get; set; }

    public int Chance { get; set; }
}

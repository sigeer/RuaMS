namespace Application.EF.Entities;

public partial class DropDataGlobalEntity
{
    private DropDataGlobalEntity() { }
    public DropDataGlobalEntity(sbyte continent, int itemid, int minimumQuantity, int maximumQuantity, int questid, int chance)
    {
        Continent = continent;
        Itemid = itemid;
        MinimumQuantity = minimumQuantity;
        MaximumQuantity = maximumQuantity;
        Questid = questid;
        Chance = chance;
    }

    public long Id { get; set; }

    public sbyte Continent { get; set; } = -1;

    public int Itemid { get; set; }

    public int MinimumQuantity { get; set; }

    public int MaximumQuantity { get; set; }

    public int Questid { get; set; }

    public int Chance { get; set; }

    public string? Comments { get; set; }
}

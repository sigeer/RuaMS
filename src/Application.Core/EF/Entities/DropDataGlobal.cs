namespace Application.EF.Entities;

public partial class DropDataGlobal
{
    public long Id { get; set; }

    public sbyte Continent { get; set; } = -1;

    public int Itemid { get; set; }

    public int MinimumQuantity { get; set; }

    public int MaximumQuantity { get; set; }

    public int Questid { get; set; }

    public int Chance { get; set; }

    public string? Comments { get; set; }
}

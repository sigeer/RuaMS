namespace Application.EF.Entities;

public partial class Inventorymerchant
{
    public int Inventorymerchantid { get; set; }

    public int Inventoryitemid { get; set; }

    public int? Characterid { get; set; }

    public int Bundles { get; set; }
}

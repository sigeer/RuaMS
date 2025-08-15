namespace Application.EF.Entities;

public partial class Inventoryitem
{
    public int Inventoryitemid { get; set; }

    public byte Type { get; set; }

    public int? Characterid { get; set; }

    public int? Accountid { get; set; }

    public int Itemid { get; set; }

    public sbyte Inventorytype { get; set; }

    public short Position { get; set; }

    public short Quantity { get; set; }

    public string Owner { get; set; } = "";

    public long Petid { get; set; } = -1;

    public short Flag { get; set; }

    public long Expiration { get; set; } = -1;

    public string GiftFrom { get; set; } = "";
}

namespace Application.EF.Entities;

public partial class Shopitem
{
    public int Shopitemid { get; set; }

    public int Shopid { get; set; }

    public int ItemId { get; set; }

    public int Price { get; set; }

    public int Pitch { get; set; }

    /// <summary>
    /// sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.
    /// </summary>
    public int Position { get; set; }
}

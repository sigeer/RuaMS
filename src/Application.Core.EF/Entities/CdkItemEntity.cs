namespace Application.EF.Entities;

public partial class CdkItemEntity
{
    private CdkItemEntity() { }
    public CdkItemEntity(int codeid, int type, int item, int quantity)
    {
        CodeId = codeid;
        Type = type;
        ItemId = item;
        Quantity = quantity;
    }

    public int Id { get; set; }

    public int CodeId { get; set; }
    /// <summary>
    /// 0.meso 1.maplepoint 2.nxPrepaid 3. ？ 4.nxcredit other. itemid
    /// </summary>
    public int Type { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }
}

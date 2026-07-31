namespace Application.EF.Entities;

public partial class ShopEntity
{
    private ShopEntity() { }
    public ShopEntity(int npcId)
    {
        NpcId = npcId;
    }

    public int ShopId { get; set; }

    public int NpcId { get; set; }
}

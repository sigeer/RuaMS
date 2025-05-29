namespace Application.EF.Entities;

public partial class Ring_Entity
{
    public Ring_Entity(long id, int itemId, long partnerRingId, int partnerChrId, string partnerName)
    {
        Id = id;
        ItemId = itemId;
        PartnerRingId = partnerRingId;
        PartnerChrId = partnerChrId;
        PartnerName = partnerName;
    }

    private Ring_Entity()
    {
    }

    public long Id { get; set; }
    public int ItemId { get; set; }

    public long PartnerRingId { get; set; }

    public int PartnerChrId { get; set; }
    public string PartnerName { get; set; } = null!;
}

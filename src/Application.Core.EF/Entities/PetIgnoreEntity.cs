namespace Application.EF.Entities;

public partial class PetIgnoreEntity
{
    protected PetIgnoreEntity() { }
    public PetIgnoreEntity(long petid, int itemid, int characterId)
    {
        Petid = petid;
        Itemid = itemid;
        CharacterId = characterId;
    }

    public int Id { get; set; }

    public long Petid { get; set; }

    public int Itemid { get; set; }
    public int CharacterId { get; set; }

    public virtual PetEntity Pet { get; set; } = null!;
}

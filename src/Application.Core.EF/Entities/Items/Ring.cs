namespace Application.EF.Entities;

public partial class Ring_Entity
{    public Ring_Entity(int id, int itemId, long ringId1, long ringId2, int characterId1, int characterId2)
    {
        Id = id;
        ItemId = itemId;
        RingId1 = ringId1;
        RingId2 = ringId2;
        CharacterId1 = characterId1;
        CharacterId2 = characterId2;
    }

    private Ring_Entity()
    {
    }

    public int Id { get; set; }
    public int ItemId { get; set; }

    public long RingId1 { get; set; }
    public long RingId2 { get; set; }

    public int CharacterId1 { get; set; }
    public int CharacterId2 { get; set; }
}

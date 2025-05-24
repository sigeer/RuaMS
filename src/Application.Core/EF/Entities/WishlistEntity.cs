namespace Application.EF.Entities;

public partial class WishlistEntity
{
    private WishlistEntity()
    {
    }

    public WishlistEntity(int charId, int sn)
    {
        CharId = charId;
        Sn = sn;
    }

    public int Id { get; set; }

    public int CharId { get; set; }

    public int Sn { get; set; }
}

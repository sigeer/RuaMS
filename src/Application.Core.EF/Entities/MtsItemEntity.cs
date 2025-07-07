namespace Application.EF.Entities;

public partial class MtsItemEntity
{
    public MtsItemEntity(int tab, int type, int itemid, int quantity, long expiration, string giftFrom, int seller, int price, string owner, string sellername, string sellEnds)
    {
        Tab = tab;
        Type = type;
        Itemid = itemid;
        Quantity = quantity;
        Expiration = expiration;
        GiftFrom = giftFrom;
        Seller = seller;
        Price = price;
        Owner = owner;
        Sellername = sellername;
        SellEnds = sellEnds;
    }

    public MtsItemEntity(int tab, int type, int itemid, int quantity, long expiration, string giftFrom, int seller, int price, int upgradeslots, int level, int str, int dex, int @int, int luk, int hp, int mp, int watk, int matk, int wdef, int mdef, int acc, int avoid, int hands, int speed, int jump, int locked, string owner, string sellername, string sellEnds, int vicious, int flag, int itemexp, int itemlevel, long ringid)
    {
        Tab = tab;
        Type = type;
        Itemid = itemid;
        Quantity = quantity;
        Seller = seller;
        Price = price;
        Upgradeslots = upgradeslots;
        Level = level;
        Itemlevel = itemlevel;
        Itemexp = itemexp;
        Ringid = ringid;
        Str = str;
        Dex = dex;
        Int = @int;
        Luk = luk;
        Hp = hp;
        Mp = mp;
        Watk = watk;
        Matk = matk;
        Wdef = wdef;
        Mdef = mdef;
        Acc = acc;
        Avoid = avoid;
        Hands = hands;
        Speed = speed;
        Jump = jump;
        Locked = locked;
        Owner = owner;
        Sellername = sellername;
        SellEnds = sellEnds;
        Vicious = vicious;
        Flag = flag;
        Expiration = expiration;
        GiftFrom = giftFrom;
    }

    protected MtsItemEntity()
    {
    }

    public int Id { get; set; }

    public int Tab { get; set; }

    public int Type { get; set; }

    public int Itemid { get; set; }

    public int Quantity { get; set; }

    public int Seller { get; set; }

    public int Price { get; set; }

    public int BidIncre { get; set; }

    public int BuyNow { get; set; }

    public int Position { get; set; }

    public int Upgradeslots { get; set; }

    public int Level { get; set; }

    public int Itemlevel { get; set; }

    public int Itemexp { get; set; }

    public long Ringid { get; set; }

    public int Str { get; set; }

    public int Dex { get; set; }

    public int Int { get; set; }

    public int Luk { get; set; }

    public int Hp { get; set; }

    public int Mp { get; set; }

    public int Watk { get; set; }

    public int Matk { get; set; }

    public int Wdef { get; set; }

    public int Mdef { get; set; }

    public int Acc { get; set; }

    public int Avoid { get; set; }

    public int Hands { get; set; }

    public int Speed { get; set; }

    public int Jump { get; set; }

    public int Locked { get; set; }

    public int Isequip { get; set; }

    public string Owner { get; set; } = null!;

    public string Sellername { get; set; } = null!;

    public string SellEnds { get; set; } = null!;

    public int Transfer { get; set; }

    public int Vicious { get; set; }

    public int Flag { get; set; }

    public long Expiration { get; set; }

    public string GiftFrom { get; set; } = null!;
}

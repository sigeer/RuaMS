namespace Application.EF;

public partial class Inventoryequipment
{
    public Inventoryequipment() { }
    public Inventoryequipment(int inventoryitemid, int upgradeslots, byte level, int str, int dex, int @int, int luk, int hp, int mp, int watk, int matk, int wdef, int mdef, int acc, int avoid, int hands, int speed, int jump, int locked, int vicious, byte itemlevel, int itemexp, int ringId)
    {
        Inventoryitemid = inventoryitemid;
        Upgradeslots = upgradeslots;
        Level = level;
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
        Vicious = vicious;
        Itemlevel = itemlevel;
        Itemexp = itemexp;
        RingId = ringId;
    }

    public int Inventoryequipmentid { get; set; }

    public int Inventoryitemid { get; set; }

    public int Upgradeslots { get; set; }

    public byte Level { get; set; }

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

    public int Vicious { get; set; }

    public byte Itemlevel { get; set; }

    public int Itemexp { get; set; }

    public int RingId { get; set; } = -1;
}

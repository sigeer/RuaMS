namespace Application.EF;

public class MonsterbookEntity
{
    protected MonsterbookEntity() { }
    public MonsterbookEntity(int charid, int cardid, int level)
    {
        Charid = charid;
        Cardid = cardid;
        Level = level;
    }

    public int Charid { get; set; }

    public int Cardid { get; set; }

    public int Level { get; set; }
}

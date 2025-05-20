namespace Application.EF.Entities;

public partial class KeyMapEntity
{
    protected KeyMapEntity() { }
    public KeyMapEntity(int characterid, int key, int type, int action)
    {
        Characterid = characterid;
        Key = key;
        Type = type;
        Action = action;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public int Key { get; set; }

    public int Type { get; set; }

    public int Action { get; set; }
}

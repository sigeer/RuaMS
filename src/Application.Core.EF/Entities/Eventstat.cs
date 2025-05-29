namespace Application.EF.Entities;

public partial class Eventstat
{
    private Eventstat()
    {
    }

    public Eventstat(int characterId, string name, int info)
    {
        Characterid = characterId;
        Name = name;
        Info = info;
    }

    public int Characterid { get; set; }

    /// <summary>
    /// 0
    /// </summary>
    public string Name { get; set; } = null!;

    public int Info { get; set; }
}

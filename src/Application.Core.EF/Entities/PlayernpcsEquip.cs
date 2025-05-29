namespace Application.EF.Entities;

public partial class PlayernpcsEquip
{
    public int Id { get; set; }

    public int Npcid { get; set; }

    public int Equipid { get; set; }

    public int Type { get; set; }

    public int Equippos { get; set; }
}

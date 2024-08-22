namespace Application.EF.Entities;

public partial class Keymap
{
    public int Id { get; set; }

    public int Characterid { get; set; }

    public int Key { get; set; }

    public int Type { get; set; }

    public int Action { get; set; }
}

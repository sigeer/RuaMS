namespace Application.EF.Entities;

public partial class Quickslotkeymapped
{
    protected Quickslotkeymapped() { }
    public Quickslotkeymapped(int accountid, long keymap)
    {
        Accountid = accountid;
        Keymap = keymap;
    }

    public int Accountid { get; set; }

    public long Keymap { get; set; }

    public virtual AccountEntity Account { get; set; } = null!;
}

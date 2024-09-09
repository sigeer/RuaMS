namespace Application.EF.Entities;

public partial class Quickslotkeymapped
{
    public int Accountid { get; set; }

    public long Keymap { get; set; }

    public virtual AccountEntity Account { get; set; } = null!;
}

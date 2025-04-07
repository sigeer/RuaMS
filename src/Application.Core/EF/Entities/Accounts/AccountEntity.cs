namespace Application.EF.Entities;

public partial class AccountEntity
{
    private AccountEntity() { }
    public AccountEntity(string name, string password)
    {
        Name = name;
        Password = password;
        Createdat = DateTimeOffset.Now;
        Birthday = DateTime.Now.Date;
        Nick = name;
    }
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Pin { get; set; } = null!;

    public string Pic { get; set; } = null!;

    public sbyte Loggedin { get; set; }

    public DateTimeOffset? Lastlogin { get; set; }

    public DateTimeOffset Createdat { get; set; }

    public DateTime Birthday { get; set; }

    public sbyte Banned { get; set; }

    public string? Banreason { get; set; }

    public string? Macs { get; set; }

    public int? NxCredit { get; set; }

    public int? MaplePoint { get; set; }

    public int? NxPrepaid { get; set; }

    public sbyte Characterslots { get; set; } = 3;

    public sbyte Gender { get; set; }

    public DateTimeOffset? Tempban { get; set; }

    public sbyte Greason { get; set; }

    public bool Tos { get; set; }

    public sbyte GMLevel { get; set; }
    public string? Nick { get; set; }
    public string? Email { get; set; }

    public string? Ip { get; set; }

    public string Hwid { get; set; } = null!;

    public int Language { get; set; } = 2;

    public virtual Quickslotkeymapped? Quickslotkeymapped { get; set; }
}

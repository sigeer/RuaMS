namespace Application.EF.Entities;

public partial class AccountEntity
{
    private AccountEntity() { }
    public AccountEntity(string name, string password)
    {
        Name = name;
        Password = password;
        Createdat = DateTimeOffset.UtcNow;
        Birthday = DateTime.Now.Date;
        Nick = name;
    }
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Pin { get; set; } = null!;

    public string Pic { get; set; } = null!;
    /// <summary>
    /// 账户的登录态完全可以由服务器内存控制-- 服务器停止时必定不在线
    /// </summary>
    //public sbyte Loggedin { get; set; }

    public DateTimeOffset? Lastlogin { get; set; }

    public DateTimeOffset Createdat { get; set; }

    public DateTime Birthday { get; set; }

    public int? NxCredit { get; set; }

    public int? MaplePoint { get; set; }

    public int? NxPrepaid { get; set; }

    public sbyte Characterslots { get; set; } = 3;
    /// <summary>
    /// 0. 男 1. 女
    /// </summary>
    public sbyte Gender { get; set; }

    public bool Tos { get; set; }

    public sbyte GMLevel { get; set; }
    public string? Nick { get; set; }
    public string? Email { get; set; }

    public int Language { get; set; } = 2;

    public virtual Quickslotkeymapped? Quickslotkeymapped { get; set; }
}

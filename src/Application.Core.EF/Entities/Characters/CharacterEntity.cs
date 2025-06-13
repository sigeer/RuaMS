namespace Application.EF.Entities;

public class CharacterEntity
{
    protected CharacterEntity() { }

    public CharacterEntity(int accountId, int world, string name, int level, int exp, int gachaexp, int str, int dex, int luk, int @int, int hp, int mp, int maxhp, int maxmp, int meso, int job, int skincolor, int gender, int hair, int face, int map, int spawnpoint, int ap, string sp)
    {
        AccountId = accountId;
        World = world;
        Name = name;
        Level = level;
        Exp = exp;
        Gachaexp = gachaexp;
        Str = str;
        Dex = dex;
        Luk = luk;
        Int = @int;
        Hp = hp;
        Mp = mp;
        Maxhp = maxhp;
        Maxmp = maxmp;
        Meso = meso;
        JobId = job;
        Skincolor = skincolor;
        Gender = gender;
        Hair = hair;
        Face = face;
        Ap = ap;
        Sp = sp;
        Map = map;
        Spawnpoint = spawnpoint;
        CreateDate = DateTimeOffset.UtcNow;
        LastLogoutTime = DateTimeOffset.UtcNow;
        LastExpGainTime = DateTimeOffset.UtcNow;
    }

    public int Id { get; set; }

    public int AccountId { get; set; }

    public int World { get; set; }

    public string Name { get; set; } = null!;

    public int Level { get; set; }

    public int Exp { get; set; }

    public int Gachaexp { get; set; }

    public int Str { get; set; }

    public int Dex { get; set; }

    public int Luk { get; set; }

    public int Int { get; set; }

    public int Hp { get; set; }

    public int Mp { get; set; }

    public int Maxhp { get; set; }

    public int Maxmp { get; set; }

    public int Meso { get; set; }

    public int HpMpUsed { get; set; }

    public int JobId { get; private set; }

    public int Skincolor { get; set; }

    public int Gender { get; set; }

    public int Fame { get; set; }

    public int Fquest { get; set; }

    public int Hair { get; set; }

    public int Face { get; set; }

    public int Ap { get; set; }

    public string Sp { get; set; } = null!;

    public int Map { get; set; }

    public int Spawnpoint { get; set; }


    public int BuddyCapacity { get; set; } = 25;

    public DateTimeOffset CreateDate { get; set; }

    public int Rank { get; set; }

    public int RankMove { get; set; }

    public int JobRank { get; set; }

    public int JobRankMove { get; set; }

    public int GuildId { get; set; }

    public int GuildRank { get; set; }

    public int MountLevel { get; set; }

    public int MountExp { get; set; }

    public int Mounttiredness { get; set; }

    public int Omokwins { get; set; }

    public int Omoklosses { get; set; }

    public int Omokties { get; set; }

    public int Matchcardwins { get; set; }

    public int Matchcardlosses { get; set; }

    public int Matchcardties { get; set; }

    public int MerchantMesos { get; set; }

    public bool HasMerchant { get; set; }

    public int Equipslots { get; set; }

    public int Useslots { get; set; }

    public int Setupslots { get; set; }

    public int Etcslots { get; set; }

    public int FamilyId { get; set; } = -1;

    public int Monsterbookcover { get; set; }

    public int AllianceRank { get; set; }

    public int VanquisherStage { get; set; }

    public int AriantPoints { get; set; }

    public int DojoPoints { get; set; }

    public int LastDojoStage { get; set; }

    public bool FinishedDojoTutorial { get; set; }

    public int VanquisherKills { get; set; }

    public int SummonValue { get; set; }

    public int PartnerId { get; set; }

    public int MarriageItemId { get; set; }

    public int Reborns { get; set; }

    public int Pqpoints { get; set; }

    public string DataString { get; set; } = null!;

    public DateTimeOffset LastLogoutTime { get; set; }

    public DateTimeOffset LastExpGainTime { get; set; }

    public bool PartySearch { get; set; } = true;

    public long Jailexpire { get; set; }

    public virtual ICollection<Famelog> Famelogs { get; set; } = new List<Famelog>();

    public virtual FamilyCharacter? FamilyCharacter { get; set; }

}

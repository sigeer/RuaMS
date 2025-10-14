using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Gachapons;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.EF;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    #region Entities
    public DbSet<GachaponPoolLevelChanceEntity> GachaponPoolLevelChances { get; set; }
    public DbSet<GachaponPoolEntity> GachaponPools { get; set; }
    public DbSet<GachaponPoolItemEntity> GachaponPoolItems { get; set; }
    public virtual DbSet<ExpLogRecord> ExpLogRecords { get; set; }
    public virtual DbSet<AccountEntity> Accounts { get; set; }
    public virtual DbSet<AccountBindingsEntity> AccountBindings { get; set; }
    public virtual DbSet<AccountBanEntity> AccountBans { get; set; }

    public virtual DbSet<AllianceEntity> Alliances { get; set; }

    public virtual DbSet<AreaInfo> AreaInfos { get; set; }

    public virtual DbSet<BbsReplyEntity> BbsReplies { get; set; }

    public virtual DbSet<BbsThreadEntity> BbsThreads { get; set; }

    public virtual DbSet<BosslogDaily> BosslogDailies { get; set; }

    public virtual DbSet<BosslogWeekly> BosslogWeeklies { get; set; }

    public virtual DbSet<BuddyEntity> Buddies { get; set; }

    public virtual DbSet<CharacterEntity> Characters { get; set; }

    public virtual DbSet<CooldownEntity> Cooldowns { get; set; }

    public virtual DbSet<DropDataGlobal> DropDataGlobals { get; set; }

    public virtual DbSet<DropDataEntity> DropData { get; set; }

    public virtual DbSet<Dueyitem> Dueyitems { get; set; }

    public virtual DbSet<DueyPackageEntity> Dueypackages { get; set; }

    public virtual DbSet<Eventstat> Eventstats { get; set; }

    public virtual DbSet<FamelogEntity> Famelogs { get; set; }

    public virtual DbSet<FamilyCharacterEntity> FamilyCharacters { get; set; }

    public virtual DbSet<FamilyEntitlementEntity> FamilyEntitlements { get; set; }

    public virtual DbSet<FredstorageEntity> Fredstorages { get; set; }

    public virtual DbSet<GiftEntity> Gifts { get; set; }

    public virtual DbSet<GuildEntity> Guilds { get; set; }

    public virtual DbSet<Hwidaccount> Hwidaccounts { get; set; }

    public virtual DbSet<HwidbanEntity> Hwidbans { get; set; }

    public virtual DbSet<Inventoryequipment> Inventoryequipments { get; set; }

    public virtual DbSet<Inventoryitem> Inventoryitems { get; set; }

    public virtual DbSet<IpbanEntity> Ipbans { get; set; }

    public virtual DbSet<KeyMapEntity> Keymaps { get; set; }

    public virtual DbSet<MacbanEntity> Macbans { get; set; }

    public virtual DbSet<Macfilter> Macfilters { get; set; }

    public virtual DbSet<MakerCreatedataEntity> Makercreatedata { get; set; }

    public virtual DbSet<MakerReagentdataEntity> Makerreagentdata { get; set; }

    public virtual DbSet<MakerRecipedataEntity> Makerrecipedata { get; set; }

    public virtual DbSet<MakerRewardDataEntity> Makerrewarddata { get; set; }

    public virtual DbSet<MarriageEntity> Marriages { get; set; }

    public virtual DbSet<Medalmap> Medalmaps { get; set; }

    public virtual DbSet<MonsterbookEntity> Monsterbooks { get; set; }

    public virtual DbSet<Monstercarddatum> Monstercarddata { get; set; }

    public virtual DbSet<MtsCart> MtsCarts { get; set; }

    public virtual DbSet<MtsItemEntity> MtsItems { get; set; }

    public virtual DbSet<Namechange> Namechanges { get; set; }

    public virtual DbSet<NewYearCardEntity> Newyears { get; set; }

    public virtual DbSet<NoteEntity> Notes { get; set; }

    public virtual DbSet<CdkCodeEntity> CdkCodes { get; set; }

    public virtual DbSet<CdkItemEntity> CdkItems { get; set; }
    public virtual DbSet<CdkRecordEntity> CdkRecords { get; set; }

    public virtual DbSet<Nxcoupon> Nxcoupons { get; set; }

    public virtual DbSet<PetEntity> Pets { get; set; }

    public virtual DbSet<Petignore> Petignores { get; set; }

    public virtual DbSet<Playerdisease> Playerdiseases { get; set; }

    public virtual DbSet<PlayerNpcEntity> Playernpcs { get; set; }

    public virtual DbSet<PlayerNpcsEquipEntity> PlayernpcsEquips { get; set; }

    public virtual DbSet<PlayernpcsField> PlayernpcsFields { get; set; }

    public virtual DbSet<PlifeEntity> Plives { get; set; }

    public virtual DbSet<Questaction> Questactions { get; set; }

    public virtual DbSet<Questprogress> Questprogresses { get; set; }

    public virtual DbSet<Questrequirement> Questrequirements { get; set; }

    public virtual DbSet<QuestStatusEntity> Queststatuses { get; set; }

    public virtual DbSet<Quickslotkeymapped> Quickslotkeymappeds { get; set; }

    public virtual DbSet<ReactorDropEntity> Reactordrops { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<Ring_Entity> Rings { get; set; }

    public virtual DbSet<SavedLocationEntity> Savedlocations { get; set; }

    public virtual DbSet<ServerQueue> ServerQueues { get; set; }

    public virtual DbSet<ShopEntity> Shops { get; set; }

    public virtual DbSet<Shopitem> Shopitems { get; set; }

    public virtual DbSet<SkillEntity> Skills { get; set; }

    public virtual DbSet<SkillMacroEntity> Skillmacros { get; set; }

    public virtual DbSet<SpecialCashItemEntity> Specialcashitems { get; set; }

    public virtual DbSet<StorageEntity> Storages { get; set; }

    public virtual DbSet<Trocklocation> Trocklocations { get; set; }

    public virtual DbSet<WishlistEntity> Wishlists { get; set; }
    #endregion

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseMySQL();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isMysql = Database.ProviderName!.Contains("mysql", StringComparison.OrdinalIgnoreCase);
        ConfigAccountCharacter(modelBuilder);

        modelBuilder.Entity<AllianceEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("alliance");

            entity.HasIndex(e => e.Name, "name");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Capacity)
                .HasDefaultValueSql("'2'")
                .HasColumnName("capacity");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasColumnName("name");
            entity.Property(e => e.Notice)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("notice");
            entity.Property(e => e.Rank1)
                .HasMaxLength(11)
                .HasDefaultValueSql("'Master'")
                .HasColumnName("rank1");
            entity.Property(e => e.Rank2)
                .HasMaxLength(11)
                .HasDefaultValueSql("'Jr. Master'")
                .HasColumnName("rank2");
            entity.Property(e => e.Rank3)
                .HasMaxLength(11)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank3");
            entity.Property(e => e.Rank4)
                .HasMaxLength(11)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank4");
            entity.Property(e => e.Rank5)
                .HasMaxLength(11)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank5");
        });

        modelBuilder.Entity<AreaInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("area_info");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Area)
                .HasColumnType("int")
                .HasColumnName("area");
            entity.Property(e => e.Charid)
                .HasColumnType("int")
                .HasColumnName("charid");
            entity.Property(e => e.Info)
                .HasMaxLength(200)
                .HasColumnName("info");
        });

        modelBuilder.Entity<BbsReplyEntity>(entity =>
        {
            entity.HasKey(e => e.Replyid).HasName("PRIMARY");

            entity.ToTable("bbs_replies");

            entity.Property(e => e.Replyid)
                .HasColumnName("replyid");
            entity.Property(e => e.Content)
                .HasMaxLength(26)
                .HasDefaultValueSql("''")
                .HasColumnName("content");
            entity.Property(e => e.Postercid)
                .HasColumnType("int")
                .HasColumnName("postercid");
            entity.Property(e => e.Threadid)
                .HasColumnType("int")
                .HasColumnName("threadid");
            entity.Property(e => e.Timestamp)
                .HasColumnType("bigint")
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<BbsThreadEntity>(entity =>
        {
            entity.HasKey(e => e.Threadid).HasName("PRIMARY");

            entity.ToTable("bbs_threads");

            entity.Property(e => e.Threadid)
                .HasColumnName("threadid");
            entity.Property(e => e.Guildid)
                .HasColumnType("int")
                .HasColumnName("guildid");
            entity.Property(e => e.Icon)
                .HasColumnType("smallint")
                .HasColumnName("icon");
            entity.Property(e => e.Localthreadid)
                .HasColumnType("int")
                .HasColumnName("localthreadid");
            entity.Property(e => e.Name)
                .HasMaxLength(26)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Postercid)
                .HasColumnType("int")
                .HasColumnName("postercid");
            entity.Property(e => e.Replycount)
                .HasColumnType("smallint")
                .HasColumnName("replycount");
            entity.Property(e => e.Startpost)
                .HasColumnType("text")
                .HasColumnName("startpost");
            entity.Property(e => e.Timestamp)
                .HasColumnType("bigint")
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<BosslogDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bosslog_daily");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Attempttime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime,v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("attempttime");
            entity.Property(e => e.Bosstype)
                .HasColumnType(isMysql ? "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')" : "text")
                .HasColumnName("bosstype");
            entity.Property(e => e.CharacterId)
                .HasColumnType("int")
                .HasColumnName("characterid");
        });

        modelBuilder.Entity<BosslogWeekly>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bosslog_weekly");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Attempttime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("attempttime");
            entity.Property(e => e.Bosstype)
                .HasColumnType(isMysql ? "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')" : "text")
                .HasColumnName("bosstype");
            entity.Property(e => e.CharacterId)
                .HasColumnType("int")
                .HasColumnName("characterid");
        });

        modelBuilder.Entity<BuddyEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("buddies");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.BuddyId)
                .HasColumnType("int")
                .HasColumnName("buddyid");
            entity.Property(e => e.CharacterId)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Group)
                .HasMaxLength(17)
                .HasDefaultValueSql("'0'")
                .HasColumnName("group");
            entity.Property(e => e.Pending)
                .HasColumnType("tinyint")
                .HasColumnName("pending");
        });

        modelBuilder.Entity<CooldownEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cooldowns");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Charid)
                .HasColumnType("int")
                .HasColumnName("charid");
            entity.Property(e => e.Length)
                .HasColumnType("bigint")
                .HasColumnName("length");
            entity.Property(e => e.SkillId)
                .HasColumnType("int")
                .HasColumnName("SkillID");
            entity.Property(e => e.StartTime).HasColumnType("bigint");
        });

        modelBuilder.Entity<DropDataGlobal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("drop_data_global");

            entity.HasIndex(e => e.Continent, "mobid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chance)
                .HasColumnType("int")
                .HasColumnName("chance");
            entity.Property(e => e.Comments)
                .HasMaxLength(45)
                .HasColumnName("comments");
            entity.Property(e => e.Continent)
                .IsRequired()
                .HasDefaultValueSql("'-1'")
                .HasColumnName("continent")
                .HasColumnType("tinyint");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.MaximumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("maximum_quantity");
            entity.Property(e => e.MinimumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("minimum_quantity");
            entity.Property(e => e.Questid)
                .HasColumnType("int")
                .HasColumnName("questid");
        });

        modelBuilder.Entity<DropDataEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("drop_data");

            entity.HasIndex(e => new { e.Dropperid, e.Itemid }, "dropperid").IsUnique();

            entity.HasIndex(e => new { e.Dropperid, e.Itemid }, "dropperid_2");

            entity.HasIndex(e => e.Dropperid, "mobid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chance)
                .HasColumnType("int")
                .HasColumnName("chance");
            entity.Property(e => e.Dropperid)
                .HasColumnType("int")
                .HasColumnName("dropperid");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.MaximumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("maximum_quantity");
            entity.Property(e => e.MinimumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("minimum_quantity");
            entity.Property(e => e.Questid)
                .HasColumnType("int")
                .HasColumnName("questid");
        });

        modelBuilder.Entity<Dueyitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dueyitems");

            entity.HasIndex(e => e.Inventoryitemid, "INVENTORYITEMID");

            entity.HasIndex(e => e.PackageId, "PackageId");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Inventoryitemid)
                .HasColumnType("int")
                .HasColumnName("inventoryitemid");
            entity.Property(e => e.PackageId).HasColumnType("int");

            entity.HasOne(d => d.Package).WithMany(p => p.Dueyitems)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("dueyitems_ibfk_1");
        });

        modelBuilder.Entity<DueyPackageEntity>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PRIMARY");

            entity.ToTable("dueypackages");

            entity.Property(e => e.PackageId);
            entity.Property(e => e.Checked)
                .HasDefaultValue(true)
                .HasSentinel(true)
                .HasColumnType("tinyint(1)");
            entity.Property(e => e.Mesos)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int");
            entity.Property(e => e.Message).HasMaxLength(200);
            entity.Property(e => e.ReceiverId).HasColumnType("int");
            entity.Property(e => e.SenderId).HasColumnType("int");
            entity.Property(e => e.TimeStamp)
                .HasDefaultValueSql("'2015-01-01 05:00:00'")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero));
            entity.Property(e => e.Type)
                .HasDefaultValue(false)
                .HasColumnType("tinyint(1)");
        });

        modelBuilder.Entity<Eventstat>(entity =>
        {
            entity.HasKey(e => e.Characterid).HasName("PRIMARY");

            entity.ToTable("eventstats");

            entity.Property(e => e.Characterid)
                .HasColumnName("characterid");
            entity.Property(e => e.Info)
                .HasColumnType("int")
                .HasColumnName("info");
            entity.Property(e => e.Name)
                .HasMaxLength(11)
                .HasDefaultValueSql("'0'")
                .HasComment("0")
                .HasColumnName("name");
        });

        modelBuilder.Entity<FamelogEntity>(entity =>
        {
            entity.HasKey(e => e.Famelogid).HasName("PRIMARY");

            entity.ToTable("famelog");

            entity.HasIndex(e => e.Characterid, "characterid");

            entity.Property(e => e.Famelogid)
                .HasColumnName("famelogid");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.CharacteridTo)
                .HasColumnType("int")
                .HasColumnName("characterid_to");
            entity.Property(e => e.When)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("when");

            entity.HasOne(d => d.Character).WithMany(p => p.Famelogs)
                .HasForeignKey(d => d.Characterid)
                .HasConstraintName("famelog_ibfk_1");
        });

        modelBuilder.Entity<FamilyCharacterEntity>(entity =>
        {
            entity.HasKey(e => e.Cid).HasName("PRIMARY");

            entity.ToTable("family_character");

            entity.HasIndex(e => new { e.Cid, e.Familyid }, "cid");

            entity.Property(e => e.Cid)
                .HasColumnName("cid");
            entity.Property(e => e.Familyid)
                .HasColumnType("int")
                .HasColumnName("familyid");
            entity.Property(e => e.Lastresettime)
                .HasColumnType("bigint")
                .HasColumnName("lastresettime");
            entity.Property(e => e.Precepts)
                .HasMaxLength(200)
                .HasColumnName("precepts");
            entity.Property(e => e.Reptosenior)
                .HasColumnType("int")
                .HasColumnName("reptosenior");
            entity.Property(e => e.Reputation)
                .HasColumnType("int")
                .HasColumnName("reputation");
            entity.Property(e => e.Seniorid)
                .HasColumnType("int")
                .HasColumnName("seniorid");
            entity.Property(e => e.Todaysrep)
                .HasColumnType("int")
                .HasColumnName("todaysrep");
            entity.Property(e => e.Totalreputation)
                .HasColumnType("int")
                .HasColumnName("totalreputation");

            entity.HasOne(d => d.CidNavigation).WithOne(p => p.FamilyCharacter)
                .HasForeignKey<FamilyCharacterEntity>(d => d.Cid)
                .HasConstraintName("family_character_ibfk_1");
        });

        modelBuilder.Entity<FamilyEntitlementEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("family_entitlement");

            entity.HasIndex(e => e.Charid, "charid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Charid)
                .HasColumnType("int")
                .HasColumnName("charid");
            entity.Property(e => e.Entitlementid)
                .HasColumnType("int")
                .HasColumnName("entitlementid");
            entity.Property(e => e.Timestamp)
                .HasColumnType("bigint")
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<FredstorageEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("fredstorage");

            entity.HasIndex(e => e.Cid, "cid_2").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cid)
                .HasColumnType("int")
                .HasColumnName("cid");
            entity.Property(e => e.Daynotes)
                .HasColumnType("int")
                .HasColumnName("daynotes");
            entity.Property(e => e.Meso)
                .HasColumnType("int")
                .HasColumnName("meso");
            entity.Property(e => e.ItemMeso)
                .HasColumnType("bigint")
                .HasColumnName("itemMeso");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<GiftEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("gifts");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.FromId)
                .HasColumnType("int")
                .HasColumnName("fromId");
            entity.Property(e => e.Message)
                .HasColumnType("tinytext")
                .HasColumnName("message");
            entity.Property(e => e.RingSourceId)
                .HasColumnType("int")
                .HasColumnName("ringSourceId");
            entity.Property(e => e.Sn)
                .HasColumnType("int")
                .HasColumnName("sn");
            entity.Property(e => e.ToId)
                .HasColumnType("int")
                .HasColumnName("toId");
        });

        modelBuilder.Entity<GuildEntity>(entity =>
        {
            entity.HasKey(e => e.GuildId).HasName("PRIMARY");

            entity.ToTable("guilds");

            entity.HasIndex(e => new { e.GuildId, e.Name }, "guildid");

            entity.Property(e => e.GuildId)
                .HasColumnName("guildid");
            entity.Property(e => e.AllianceId)
                .HasColumnType("int")
                .HasColumnName("allianceId");
            entity.Property(e => e.Capacity)
                .HasDefaultValueSql("'10'")
                .HasColumnType("int")
                .HasColumnName("capacity");
            entity.Property(e => e.GP)
                .HasColumnType("int")
                .HasColumnName("GP");
            entity.Property(e => e.Leader)
                .HasColumnType("int")
                .HasColumnName("leader");
            entity.Property(e => e.Logo)
                .HasColumnType("int")
                .HasColumnName("logo");
            entity.Property(e => e.LogoBg)
                .HasColumnType("int")
                .HasColumnName("logoBG");
            entity.Property(e => e.LogoBgColor)
                .HasColumnType("smallint")
                .HasColumnName("logoBGColor");
            entity.Property(e => e.LogoColor)
                .HasColumnType("smallint")
                .HasColumnName("logoColor");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Notice)
                .HasMaxLength(101)
                .HasColumnName("notice");
            entity.Property(e => e.Rank1Title)
                .HasMaxLength(45)
                .HasDefaultValueSql("'Master'")
                .HasColumnName("rank1title");
            entity.Property(e => e.Rank2Title)
                .HasMaxLength(45)
                .HasDefaultValueSql("'Jr. Master'")
                .HasColumnName("rank2title");
            entity.Property(e => e.Rank3Title)
                .HasMaxLength(45)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank3title");
            entity.Property(e => e.Rank4Title)
                .HasMaxLength(45)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank4title");
            entity.Property(e => e.Rank5Title)
                .HasMaxLength(45)
                .HasDefaultValueSql("'Member'")
                .HasColumnName("rank5title");
            entity.Property(e => e.Signature)
                .HasColumnType("int")
                .HasColumnName("signature");
        });

        modelBuilder.Entity<Hwidaccount>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.Hwid }).HasName("PRIMARY");

            entity.ToTable("hwidaccounts");

            entity.Property(e => e.AccountId)
                .HasColumnName("accountid");
            entity.Property(e => e.Hwid)
                .HasMaxLength(40)
                .HasDefaultValueSql("''")
                .HasColumnName("hwid");
            entity.Property(e => e.ExpiresAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("expiresat");
            entity.Property(e => e.Relevance)
                .HasColumnType("tinyint")
                .HasColumnName("relevance");
        });

        modelBuilder.Entity<HwidbanEntity>(entity =>
        {
            entity.HasKey(e => e.Hwidbanid).HasName("PRIMARY");

            entity.ToTable("hwidbans");

            entity.HasIndex(e => e.Hwid, "hwid_2").IsUnique();

            entity.Property(e => e.Hwidbanid)
                .HasColumnName("hwidbanid");
            entity.Property(e => e.Hwid)
                .HasMaxLength(30)
                .HasColumnName("hwid");

            entity.Property(e => e.AccountId)
                .HasColumnName("AccountId");
        });

        ConfigInventory(modelBuilder);

        modelBuilder.Entity<IpbanEntity>(entity =>
        {
            entity.HasKey(e => e.Ipbanid).HasName("PRIMARY");

            entity.ToTable("ipbans");

            entity.Property(e => e.Ipbanid)
                .HasColumnName("ipbanid");
            entity.Property(e => e.Aid)
                .HasColumnName("aid");
            entity.Property(e => e.Ip)
                .HasMaxLength(40)
                .HasDefaultValueSql("''")
                .HasColumnName("ip");
        });

        modelBuilder.Entity<KeyMapEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("keymap");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasColumnType("int")
                .HasColumnName("action");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Key)
                .HasColumnType("int")
                .HasColumnName("key");
            entity.Property(e => e.Type)
                .HasColumnType("int")
                .HasColumnName("type");
        });

        modelBuilder.Entity<MacbanEntity>(entity =>
        {
            entity.HasKey(e => e.Macbanid).HasName("PRIMARY");

            entity.ToTable("macbans");

            entity.HasIndex(e => e.Mac, "mac_2").IsUnique();

            entity.Property(e => e.Macbanid)
                .HasColumnName("macbanid");
            entity.Property(e => e.Aid)
                .HasColumnName("aid");
            entity.Property(e => e.Mac)
                .HasMaxLength(30)
                .HasColumnName("mac");
        });

        modelBuilder.Entity<Macfilter>(entity =>
        {
            entity.HasKey(e => e.Macfilterid).HasName("PRIMARY");

            entity.ToTable("macfilters");

            entity.Property(e => e.Macfilterid)
                .HasColumnName("macfilterid");
            entity.Property(e => e.Filter)
                .HasMaxLength(30)
                .HasColumnName("filter");
        });

        modelBuilder.Entity<MakerCreatedataEntity>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Itemid }).HasName("PRIMARY");

            entity.ToTable("makercreatedata");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Catalyst)
                .HasColumnType("int")
                .HasColumnName("catalyst");
            entity.Property(e => e.Quantity)
                .HasColumnType("smallint")
                .HasColumnName("quantity");
            entity.Property(e => e.ReqEquip)
                .HasColumnType("int")
                .HasColumnName("req_equip");
            entity.Property(e => e.ReqItem)
                .HasColumnType("int")
                .HasColumnName("req_item");
            entity.Property(e => e.ReqLevel)
                .HasColumnType("smallint")
                .HasColumnName("req_level");
            entity.Property(e => e.ReqMakerLevel)
                .HasColumnType("smallint")
                .HasColumnName("req_maker_level");
            entity.Property(e => e.ReqMeso)
                .HasColumnType("int")
                .HasColumnName("req_meso");
            entity.Property(e => e.Tuc)
                .HasColumnType("tinyint")
                .HasColumnName("tuc");
        });

        modelBuilder.Entity<MakerReagentdataEntity>(entity =>
        {
            entity.HasKey(e => e.Itemid).HasName("PRIMARY");

            entity.ToTable("makerreagentdata");

            entity.Property(e => e.Itemid)
                .HasColumnName("itemid");
            entity.Property(e => e.Stat)
                .HasMaxLength(20)
                .HasColumnName("stat");
            entity.Property(e => e.Value)
                .HasColumnType("smallint")
                .HasColumnName("value");
        });

        modelBuilder.Entity<MakerRecipedataEntity>(entity =>
        {
            entity.HasKey(e => new { e.Itemid, e.ReqItem }).HasName("PRIMARY");

            entity.ToTable("makerrecipedata");

            entity.Property(e => e.Itemid)
                .HasColumnName("itemid");
            entity.Property(e => e.ReqItem)
                .HasColumnType("int")
                .HasColumnName("req_item");
            entity.Property(e => e.Count)
                .HasColumnType("smallint")
                .HasColumnName("count");
        });

        modelBuilder.Entity<MakerRewardDataEntity>(entity =>
        {
            entity.HasKey(e => new { e.Itemid, e.Rewardid }).HasName("PRIMARY");

            entity.ToTable("makerrewarddata");

            entity.Property(e => e.Itemid)
                .HasColumnName("itemid");
            entity.Property(e => e.Rewardid)
                .HasColumnName("rewardid");
            entity.Property(e => e.Prob)
                .HasDefaultValueSql("'100'")
                .HasColumnType("tinyint")
                .HasColumnName("prob");
            entity.Property(e => e.Quantity)
                .HasColumnType("smallint")
                .HasColumnName("quantity");
        });

        modelBuilder.Entity<MarriageEntity>(entity =>
        {
            entity.HasKey(e => e.Marriageid).HasName("PRIMARY");

            entity.ToTable("marriages");

            entity.Property(e => e.Marriageid)
                .HasColumnName("marriageid");
            entity.Property(e => e.Husbandid)
                .HasColumnType("int")
                .HasColumnName("husbandid");
            entity.Property(e => e.Wifeid)
                .HasColumnType("int")
                .HasColumnName("wifeid");
        });

        modelBuilder.Entity<Medalmap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("medalmaps");

            entity.HasIndex(e => e.Queststatusid, "queststatusid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Mapid)
                .HasColumnType("int")
                .HasColumnName("mapid");
            entity.Property(e => e.Queststatusid)
                .HasColumnType("int")
                .HasColumnName("queststatusid");
        });

        modelBuilder.Entity<MonsterbookEntity>(entity =>
        {
            entity.HasKey(e => new { e.Cardid, e.Charid });
            entity
                .ToTable("monsterbook");

            entity.Property(e => e.Cardid)
                .HasColumnName("cardid");
            entity.Property(e => e.Charid)
                .HasColumnName("charid");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("level");
        });

        modelBuilder.Entity<Monstercarddatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("monstercarddata");

            entity.HasIndex(e => e.Id, "id").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cardid)
                .HasColumnType("int")
                .HasColumnName("cardid");
            entity.Property(e => e.Mobid)
                .HasColumnType("int")
                .HasColumnName("mobid");
        });

        modelBuilder.Entity<MtsCart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mts_cart");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cid)
                .HasColumnType("int")
                .HasColumnName("cid");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
        });

        modelBuilder.Entity<MtsItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mts_items");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Acc)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("acc");
            entity.Property(e => e.Avoid)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("avoid");
            entity.Property(e => e.BidIncre)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("bid_incre");
            entity.Property(e => e.BuyNow)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("buy_now");
            entity.Property(e => e.Dex)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("dex");
            entity.Property(e => e.Expiration)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("expiration");
            entity.Property(e => e.Flag)
                .HasColumnType("int")
                .HasColumnName("flag");
            entity.Property(e => e.GiftFrom)
                .HasMaxLength(26)
                .HasColumnName("giftFrom");
            entity.Property(e => e.Hands)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("hands");
            entity.Property(e => e.Hp)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("hp");
            entity.Property(e => e.Int)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("int");
            entity.Property(e => e.Isequip)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("isequip");
            entity.Property(e => e.Itemexp)
                .HasColumnType("int")
                .HasColumnName("itemexp");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Itemlevel)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("itemlevel");
            entity.Property(e => e.Jump)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("jump");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("level");
            entity.Property(e => e.Locked)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("locked");
            entity.Property(e => e.Luk)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("luk");
            entity.Property(e => e.Matk)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("matk");
            entity.Property(e => e.Mdef)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("mdef");
            entity.Property(e => e.Mp)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("mp");
            entity.Property(e => e.Owner)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .HasColumnName("owner");
            entity.Property(e => e.Position)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("position");
            entity.Property(e => e.Price)
                .HasColumnType("int")
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("quantity");
            entity.Property(e => e.Ringid)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("ringid");
            entity.Property(e => e.SellEnds)
                .HasMaxLength(16)
                .HasColumnName("sell_ends");
            entity.Property(e => e.Seller)
                .HasColumnType("int")
                .HasColumnName("seller");
            entity.Property(e => e.Sellername)
                .HasMaxLength(16)
                .HasColumnName("sellername");
            entity.Property(e => e.Speed)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("speed");
            entity.Property(e => e.Str)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("str");
            entity.Property(e => e.Tab)
                .HasColumnType("int")
                .HasColumnName("tab");
            entity.Property(e => e.Transfer)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("transfer");
            entity.Property(e => e.Type)
                .HasColumnType("int")
                .HasColumnName("type");
            entity.Property(e => e.Upgradeslots)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("upgradeslots");
            entity.Property(e => e.Vicious)
                .HasColumnType("int")
                .HasColumnName("vicious");
            entity.Property(e => e.Watk)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("watk");
            entity.Property(e => e.Wdef)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int")
                .HasColumnName("wdef");
        });

        modelBuilder.Entity<Namechange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("namechanges");

            entity.HasIndex(e => e.Characterid, "characterid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.CompletionTime)
                .HasColumnType("timestamp")
                .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null
                )
                .HasColumnName("completionTime");
            entity.Property(e => e.New)
                .HasMaxLength(13)
                .HasColumnName("new");
            entity.Property(e => e.Old)
                .HasMaxLength(13)
                .HasColumnName("old");
            entity.Property(e => e.RequestTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("requestTime");
        });

        modelBuilder.Entity<NewYearCardEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("newyear");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Message)
                .HasMaxLength(120)
                .HasDefaultValueSql("''")
                .HasColumnName("message");
            entity.Property(e => e.Received).HasColumnName("received");
            entity.Property(e => e.ReceiverDiscard).HasColumnName("receiverdiscard");
            entity.Property(e => e.ReceiverId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("int")
                .HasColumnName("receiverid");
            entity.Property(e => e.SenderDiscard).HasColumnName("senderdiscard");
            entity.Property(e => e.SenderId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("int")
                .HasColumnName("senderid");
            entity.Property(e => e.TimeReceived)
                .HasColumnType("bigint")
                .HasColumnName("timereceived");
            entity.Property(e => e.TimeSent)
                .HasColumnType("bigint")
                .HasColumnName("timesent");
        });

        modelBuilder.Entity<NoteEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notes");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Deleted)
                .HasColumnName("deleted");
            entity.Property(e => e.Fame)
                .HasColumnType("int")
                .HasColumnName("fame");
            entity.Property(e => e.FromId)
                .HasColumnType("int")
                .HasColumnName("fromId");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.Timestamp)
                .HasColumnType("bigint")
                .HasColumnName("timestamp");
            entity.Property(e => e.ToId)
                .HasColumnType("int")
                .HasColumnName("toId");
        });

        modelBuilder.Entity<CdkCodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cdk_codes");

            entity.HasIndex(e => e.Code, "idx_code").IsUnique();

            entity.Property(e => e.Id);
            entity.Property(e => e.Code)
                .HasMaxLength(17);
            entity.Property(e => e.MaxCount)
                .HasColumnType("int");
            entity.Property(e => e.Expiration)
                .HasColumnType("bigint");
        });

        modelBuilder.Entity<CdkItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cdk_items");

            entity.HasIndex(e => e.CodeId, "idx_code");

            entity.Property(e => e.Id);
            entity.Property(e => e.CodeId)
                .HasColumnType("int");
            entity.Property(e => e.ItemId)
                .HasDefaultValueSql("'4000000'")
                .HasColumnType("int");
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int");
        });

        modelBuilder.Entity<CdkRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cdk_records");

            entity.HasIndex(e => e.CodeId, "idx_code");

            entity.Property(e => e.Id);
            entity.Property(e => e.CodeId)
                .HasColumnType("int");
            entity.Property(e => e.RecipientId)
                .HasColumnType("int");
            entity.Property(e => e.RecipientTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero));
        });

        modelBuilder.Entity<Nxcoupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("nxcoupons");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Activeday)
                .HasColumnType("int")
                .HasColumnName("activeday");
            entity.Property(e => e.CouponId)
                .HasColumnType("int")
                .HasColumnName("couponid");
            entity.Property(e => e.Endhour)
                .HasColumnType("int")
                .HasColumnName("endhour");
            entity.Property(e => e.Rate)
                .HasColumnType("int")
                .HasColumnName("rate");
            entity.Property(e => e.Starthour)
                .HasColumnType("int")
                .HasColumnName("starthour");
        });

        modelBuilder.Entity<PetEntity>(entity =>
        {
            entity.HasKey(e => e.Petid).HasName("PRIMARY");

            entity.ToTable("pets");

            var idProp = entity.Property(e => e.Petid)
                .HasColumnName("petid");

            entity.Property(e => e.Closeness)
                    .HasColumnType("int")
                    .HasColumnName("closeness");
            entity.Property(e => e.Flag)
                .HasColumnType("int")
                .HasColumnName("flag");
            entity.Property(e => e.Fullness)
                .HasColumnType("int")
                .HasColumnName("fullness");
            entity.Property(e => e.Level)
                .HasColumnType("int")
                .HasColumnName("level");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasColumnName("name");
            entity.Property(e => e.Summoned).HasColumnName("summoned");
        });

        modelBuilder.Entity<Petignore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("petignores");

            entity.HasIndex(e => e.Petid, "fk_petignorepetid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Petid)
                .HasColumnType("bigint")
                .HasColumnName("petid");

            entity.HasOne(d => d.Pet).WithMany(p => p.Petignores)
                .HasForeignKey(d => d.Petid)
                .HasConstraintName("fk_petignorepetid");
        });

        modelBuilder.Entity<Playerdisease>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playerdiseases");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Charid)
                .HasColumnType("int")
                .HasColumnName("charid");
            entity.Property(e => e.Disease)
                .HasColumnType("int")
                .HasColumnName("disease");
            entity.Property(e => e.Length)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("length");
            entity.Property(e => e.Mobskillid)
                .HasColumnType("int")
                .HasColumnName("mobskillid");
            entity.Property(e => e.Mobskilllv)
                .HasColumnType("int")
                .HasColumnName("mobskilllv");
        });

        modelBuilder.Entity<PlayerNpcEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cy)
                .HasColumnType("int")
                .HasColumnName("cy");
            entity.Property(e => e.Dir)
                .HasColumnType("int")
                .HasColumnName("dir");
            entity.Property(e => e.Face)
                .HasColumnType("int")
                .HasColumnName("face");
            entity.Property(e => e.Fh)
                .HasColumnType("int")
                .HasColumnName("fh");
            entity.Property(e => e.Gender)
                .HasColumnType("int")
                .HasColumnName("gender");
            entity.Property(e => e.Hair)
                .HasColumnType("int")
                .HasColumnName("hair");
            entity.Property(e => e.Job)
                .HasColumnType("int")
                .HasColumnName("job");
            entity.Property(e => e.Map)
                .HasColumnType("int")
                .HasColumnName("map");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasColumnName("name");
            entity.Property(e => e.Overallrank)
                .HasColumnType("int")
                .HasColumnName("overallrank");
            entity.Property(e => e.Rx0)
                .HasColumnType("int")
                .HasColumnName("rx0");
            entity.Property(e => e.Rx1)
                .HasColumnType("int")
                .HasColumnName("rx1");
            entity.Property(e => e.Scriptid)
                .HasColumnType("int")
                .HasColumnName("scriptid");
            entity.Property(e => e.Skin)
                .HasColumnType("int")
                .HasColumnName("skin");
            entity.Property(e => e.World)
                .HasColumnType("int")
                .HasColumnName("world");
            entity.Property(e => e.Worldjobrank)
                .HasColumnType("int")
                .HasColumnName("worldjobrank");
            entity.Property(e => e.Worldrank)
                .HasColumnType("int")
                .HasColumnName("worldrank");
            entity.Property(e => e.X)
                .HasColumnType("int")
                .HasColumnName("x");
        });

        modelBuilder.Entity<PlayerNpcsEquipEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs_equip");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Equipid)
                .HasColumnType("int")
                .HasColumnName("equipid");
            entity.Property(e => e.Equippos)
                .HasColumnType("int")
                .HasColumnName("equippos");
            entity.Property(e => e.Npcid)
                .HasColumnType("int")
                .HasColumnName("npcid");
            entity.Property(e => e.Type)
                .HasColumnType("int")
                .HasColumnName("type");
        });

        modelBuilder.Entity<PlayernpcsField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs_field");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Map)
                .HasColumnType("int")
                .HasColumnName("map");
            entity.Property(e => e.Podium)
                .HasColumnType("smallint")
                .HasColumnName("podium");
            entity.Property(e => e.Step).HasColumnName("step").HasColumnType("tinyint").HasDefaultValueSql("'0'");
            entity.Property(e => e.World)
                .HasColumnType("int")
                .HasColumnName("world");
        });

        modelBuilder.Entity<PlifeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plife");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cy)
                .HasColumnType("int")
                .HasColumnName("cy");
            entity.Property(e => e.F)
                .HasColumnType("int")
                .HasColumnName("f");
            entity.Property(e => e.Fh)
                .HasColumnType("int")
                .HasColumnName("fh");
            entity.Property(e => e.Hide)
                .HasColumnType("int")
                .HasColumnName("hide");
            entity.Property(e => e.Life)
                .HasColumnType("int")
                .HasColumnName("life");
            entity.Property(e => e.Map)
                .HasColumnType("int")
                .HasColumnName("map");
            entity.Property(e => e.Mobtime)
                .HasColumnType("int")
                .HasColumnName("mobtime");
            entity.Property(e => e.Rx0)
                .HasColumnType("int")
                .HasColumnName("rx0");
            entity.Property(e => e.Rx1)
                .HasColumnType("int")
                .HasColumnName("rx1");
            entity.Property(e => e.Team)
                .HasColumnType("int")
                .HasColumnName("team");
            entity.Property(e => e.Type)
                .HasMaxLength(1)
                .HasDefaultValueSql("'n'")
                .HasColumnName("type");
            entity.Property(e => e.World)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("int")
                .HasColumnName("world");
            entity.Property(e => e.X)
                .HasColumnType("int")
                .HasColumnName("x");
            entity.Property(e => e.Y)
                .HasColumnType("int")
                .HasColumnName("y");
        });

        modelBuilder.Entity<Questaction>(entity =>
        {
            entity.HasKey(e => e.Questactionid).HasName("PRIMARY");

            entity.ToTable("questactions");

            entity.Property(e => e.Questactionid)
                .HasColumnName("questactionid");
            entity.Property(e => e.Data)
                .HasColumnType("blob")
                .HasColumnName("data");
            entity.Property(e => e.Questid)
                .HasColumnType("int")
                .HasColumnName("questid");
            entity.Property(e => e.Status)
                .HasColumnType("int")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Questprogress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("questprogress");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Progress)
                .HasMaxLength(15)
                .HasDefaultValueSql("''")
                .HasColumnName("progress");
            entity.Property(e => e.Progressid)
                .HasColumnType("int")
                .HasColumnName("progressid");
            entity.Property(e => e.Queststatusid)
                .HasColumnType("int")
                .HasColumnName("queststatusid");
        });

        modelBuilder.Entity<Questrequirement>(entity =>
        {
            entity.HasKey(e => e.Questrequirementid).HasName("PRIMARY");

            entity.ToTable("questrequirements");

            entity.Property(e => e.Questrequirementid)
                .HasColumnName("questrequirementid");
            entity.Property(e => e.Data)
                .HasColumnType("blob")
                .HasColumnName("data");
            entity.Property(e => e.Questid)
                .HasColumnType("int")
                .HasColumnName("questid");
            entity.Property(e => e.Status)
                .HasColumnType("int")
                .HasColumnName("status");
        });

        modelBuilder.Entity<QuestStatusEntity>(entity =>
        {
            entity.HasKey(e => e.Queststatusid).HasName("PRIMARY");

            entity.ToTable("queststatus");

            entity.Property(e => e.Queststatusid)
                .HasColumnName("queststatusid");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Completed)
                .HasColumnType("int")
                .HasColumnName("completed");
            entity.Property(e => e.Expires)
                .HasColumnType("bigint")
                .HasColumnName("expires");
            entity.Property(e => e.Forfeited)
                .HasColumnType("int")
                .HasColumnName("forfeited");
            entity.Property(e => e.Info)
                .HasColumnType("tinyint")
                .HasColumnName("info");
            entity.Property(e => e.Quest)
                .HasColumnType("int")
                .HasColumnName("quest");
            entity.Property(e => e.Status)
                .HasColumnType("int")
                .HasColumnName("status");
            entity.Property(e => e.Time)
                .HasColumnType("int")
                .HasColumnName("time");
        });

        modelBuilder.Entity<Quickslotkeymapped>(entity =>
        {
            entity.HasKey(e => e.Accountid).HasName("PRIMARY");

            entity.ToTable("quickslotkeymapped");

            entity.Property(e => e.Accountid)
                .HasColumnName("accountid");
            entity.Property(e => e.Keymap)
                .HasColumnType("bigint")
                .HasColumnName("keymap");

            entity.HasOne(d => d.Account).WithOne(p => p.Quickslotkeymapped)
                .HasForeignKey<Quickslotkeymapped>(d => d.Accountid)
                .HasConstraintName("quickslotkeymapped_accountid_fk");
        });

        modelBuilder.Entity<ReactorDropEntity>(entity =>
        {
            entity.HasKey(e => e.Reactordropid).HasName("PRIMARY");

            entity.ToTable("reactordrops");

            entity.HasIndex(e => e.Reactorid, "reactorid");

            entity.Property(e => e.Reactordropid)
                .HasColumnName("reactordropid");
            entity.Property(e => e.Chance)
                .HasColumnType("int")
                .HasColumnName("chance");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Questid)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("int")
                .HasColumnName("questid");
            entity.Property(e => e.Reactorid)
                .HasColumnType("int")
                .HasColumnName("reactorid");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reports");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chatlog)
                .HasColumnType("text")
                .HasColumnName("chatlog");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Reason)
                .HasColumnType("tinyint")
                .HasColumnName("reason");
            entity.Property(e => e.Reporterid)
                .HasColumnType("int")
                .HasColumnName("reporterid");
            entity.Property(e => e.Reporttime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("reporttime");
            entity.Property(e => e.Victimid)
                .HasColumnType("int")
                .HasColumnName("victimid");
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("responses");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chat)
                .HasColumnType("text")
                .HasColumnName("chat");
            entity.Property(e => e.Response1)
                .HasColumnType("text")
                .HasColumnName("response");
        });

        modelBuilder.Entity<Ring_Entity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rings");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ItemId)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.CharacterId1)
                .HasColumnType("int")
                .HasColumnName("characterId1");
            entity.Property(e => e.RingId1)
                .HasColumnType("bigint")
                .HasColumnName("ringId1");
            entity.Property(e => e.CharacterId2)
                .HasColumnType("int")
                .HasColumnName("characterId2");
            entity.Property(e => e.RingId2)
                .HasColumnType("bigint")
                .HasColumnName("ringId2");
        });

        modelBuilder.Entity<SavedLocationEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("savedlocations");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");

            entity.Property(e => e.Locationtype)
             .HasColumnName("locationtype")
             .HasColumnType(isMysql ? "enum('FREE_MARKET','WORLDTOUR','FLORINA','INTRO','SUNDAY_MARKET','MIRROR','EVENT','BOSSPQ','HAPPYVILLE','DEVELOPER','MONSTER_CARNIVAL','JAIL','CYGNUSINTRO')" : "text");

            entity.Property(e => e.Map)
                .HasColumnType("int")
                .HasColumnName("map");
            entity.Property(e => e.Portal)
                .HasColumnType("int")
                .HasColumnName("portal");
        });

        modelBuilder.Entity<ServerQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("server_queue");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Accountid)
                .HasColumnType("int")
                .HasColumnName("accountid");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnType("timestamp")
                .HasColumnName("createTime");
            entity.Property(e => e.Message)
                .HasMaxLength(128)
                .HasColumnName("message");
            entity.Property(e => e.Type)
                .HasColumnType("tinyint")
                .HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("int")
                .HasColumnName("value");
        });

        modelBuilder.Entity<ShopEntity>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PRIMARY");

            entity.ToTable("shops");

            entity.Property(e => e.ShopId)
                .HasColumnName("shopid");
            entity.Property(e => e.NpcId)
                .HasColumnType("int")
                .HasColumnName("npcid");
        });

        modelBuilder.Entity<Shopitem>(entity =>
        {
            entity.HasKey(e => e.Shopitemid).HasName("PRIMARY");

            entity.ToTable("shopitems");

            entity.Property(e => e.Shopitemid)
                .HasColumnName("shopitemid");
            entity.Property(e => e.ItemId)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Pitch)
                .HasColumnType("int")
                .HasColumnName("pitch");
            entity.Property(e => e.Position)
                .HasComment("sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                .HasColumnType("int")
                .HasColumnName("position");
            entity.Property(e => e.Price)
                .HasColumnType("int")
                .HasColumnName("price");
            entity.Property(e => e.Shopid)
                .HasColumnType("int")
                .HasColumnName("shopid");
        });

        modelBuilder.Entity<SkillEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("skills");

            entity.HasIndex(e => new { e.Skillid, e.Characterid }, "skillpair").IsUnique();

            entity.HasIndex(e => e.Characterid, "skills_chrid_fk");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Expiration)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("expiration");
            entity.Property(e => e.Masterlevel)
                .HasColumnType("int")
                .HasColumnName("masterlevel");
            entity.Property(e => e.Skillid)
                .HasColumnType("int")
                .HasColumnName("skillid");
            entity.Property(e => e.Skilllevel)
                .HasColumnType("int")
                .HasColumnName("skilllevel");
        });

        modelBuilder.Entity<SkillMacroEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("skillmacros");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasColumnName("name");
            entity.Property(e => e.Position).HasColumnName("position").HasColumnType("tinyint").HasDefaultValueSql("'0'");
            entity.Property(e => e.Shout).HasColumnName("shout").HasColumnType("tinyint").HasDefaultValueSql("'0'");
            entity.Property(e => e.Skill1)
                .HasColumnType("int")
                .HasColumnName("skill1");
            entity.Property(e => e.Skill2)
                .HasColumnType("int")
                .HasColumnName("skill2");
            entity.Property(e => e.Skill3)
                .HasColumnType("int")
                .HasColumnName("skill3");
        });

        modelBuilder.Entity<SpecialCashItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("specialcashitems");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Info)
                .HasColumnType("int")
                .HasColumnName("info");
            entity.Property(e => e.Modifier)
                .HasComment("1024 is add/remove")
                .HasColumnType("int")
                .HasColumnName("modifier");
            entity.Property(e => e.Sn)
                .HasColumnType("int")
                .HasColumnName("sn");
        });

        modelBuilder.Entity<StorageEntity>(entity =>
        {
            entity.HasKey(e => e.Storageid).HasName("PRIMARY");

            entity.ToTable("storages");

            entity.Property(e => e.Storageid)
                .HasColumnName("storageid");
            entity.Property(e => e.Accountid)
                .HasColumnType("int")
                .HasColumnName("accountid");
            entity.Property(e => e.Meso)
                .HasColumnType("int")
                .HasColumnName("meso");
            entity.Property(e => e.Slots)
                .HasColumnType("int")
                .HasColumnName("slots");
        });

        modelBuilder.Entity<Trocklocation>(entity =>
        {
            entity.HasKey(e => e.Trockid).HasName("PRIMARY");

            entity.ToTable("trocklocations");

            entity.Property(e => e.Trockid)
                .HasColumnName("trockid");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Mapid)
                .HasColumnType("int")
                .HasColumnName("mapid");
            entity.Property(e => e.Vip)
                .HasColumnType("int")
                .HasColumnName("vip");
        });

        modelBuilder.Entity<WishlistEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("wishlists");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.CharId)
                .HasColumnType("int")
                .HasColumnName("charid");
            entity.Property(e => e.Sn)
                .HasColumnType("int")
                .HasColumnName("sn");
        });

        modelBuilder.Entity<ExpLogRecord>(entity =>
        {
            entity.ToTable("characterexplogs");
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.WorldExpRate)
                .HasColumnType("int")
                .HasColumnName("world_exp_rate");
            entity.Property(e => e.ExpCoupon)
                .HasColumnType("int")
                .HasColumnName("exp_coupon");
            entity.Property(e => e.GainedExp)
                .HasColumnType("bigint")
                .HasColumnName("gained_exp");
            entity.Property(e => e.CurrentExp)
                .HasColumnType("int")
                .HasColumnName("current_exp");
            entity.Property(e => e.ExpGainTime)
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("exp_gain_time");
            entity.Property(e => e.CharId)
                .HasColumnType("int")
                .HasColumnName("charid");
        });

        modelBuilder.Entity<GachaponPoolEntity>(entity =>
        {
            entity.ToTable("gachapon_pool");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Name).HasColumnType("varchar(50)").IsRequired().HasDefaultValueSql("''");
        });

        modelBuilder.Entity<GachaponPoolItemEntity>(entity =>
        {
            entity.ToTable("gachapon_pool_item");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });


        modelBuilder.Entity<GachaponPoolLevelChanceEntity>(entity =>
        {
            entity.ToTable("gachapon_pool_level_chance");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private void ConfigAccountCharacter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("accounts");

            entity.HasIndex(e => new { e.Id, e.Name }, "id");

            entity.HasIndex(e => new { e.Id, e.NxCredit, e.MaplePoint, e.NxPrepaid }, "id_2");

            entity.HasIndex(e => e.Name, "name").IsUnique();


            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Birthday)
                .HasDefaultValueSql("'2005-05-11'")
                .HasColumnType("date")
                .HasColumnName("birthday");
            entity.Property(e => e.Characterslots)
                .HasDefaultValueSql("'3'")
                .HasColumnType("tinyint")
                .HasColumnName("characterslots");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'10'")
                .HasColumnType("tinyint")
                .HasColumnName("gender");
            entity.Property(e => e.Lastlogin)
                .HasColumnType("timestamp")
                .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null
                )
                .HasColumnName("lastlogin");
            entity.Property(e => e.MaplePoint)
                .HasColumnType("int")
                .HasColumnName("maplePoint");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Nick)
                .HasMaxLength(20)
                .HasColumnName("nick");
            entity.Property(e => e.NxCredit)
                .HasColumnType("int")
                .HasColumnName("nxCredit");
            entity.Property(e => e.NxPrepaid)
                .HasColumnType("int")
                .HasColumnName("nxPrepaid");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .HasColumnName("password");
            entity.Property(e => e.Pic)
                .HasMaxLength(26)
                .HasDefaultValueSql("''")
                .HasColumnName("pic");
            entity.Property(e => e.Pin)
                .HasMaxLength(10)
                .HasDefaultValueSql("''")
                .HasColumnName("pin");
            entity.Property(e => e.GMLevel)
                .HasColumnType("tinyint")
                .HasDefaultValueSql("'0'")
                .HasColumnName("gmlevel");
            entity.Property(e => e.Tos).HasColumnName("tos");
        });

        modelBuilder.Entity<AccountBindingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("account_bindings");

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.Property(e => e.Id)
                .HasColumnName("Id");
            entity.Property(e => e.AccountId)
                .HasColumnType("int")
                .HasColumnName("AccountId");

            entity.Property(e => e.IP)
                .HasMaxLength(50)
                .HasDefaultValueSql("''")
                .HasColumnName("IP");

            entity.Property(e => e.MAC)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("MAC");

            entity.Property(e => e.HWID)
                .HasMaxLength(30)
                .HasDefaultValueSql("''")
                .HasColumnName("HWID");

            entity.Property(e => e.LastActiveTime)
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("LastActiveTime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AccountBanEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("account_ban");

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.Property(e => e.Id)
                .HasColumnName("Id");
            entity.Property(e => e.AccountId)
                .HasColumnType("int")
                .HasColumnName("AccountId");

            entity.Property(e => e.Reason)
                .HasColumnType("tinyint")
                .HasColumnName("Reason");

            entity.Property(e => e.ReasonDescription)
                .HasColumnType("text")
                .HasColumnName("ReasonDescription");

            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("StartTime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("EndTime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("characters");

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.HasIndex(e => new { e.Id, e.AccountId, e.World }, "id");

            entity.HasIndex(e => new { e.Id, e.AccountId, e.Name }, "id_2");

            entity.HasIndex(e => new { e.Level, e.Exp }, "ranking1");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.AccountId)
                .HasColumnType("int")
                .HasColumnName("accountid");
            entity.Property(e => e.AllianceRank)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int")
                .HasColumnName("allianceRank");
            entity.Property(e => e.Ap)
                .HasColumnType("int")
                .HasColumnName("ap");
            entity.Property(e => e.AriantPoints)
                .HasColumnType("int")
                .HasColumnName("ariantPoints");
            entity.Property(e => e.BuddyCapacity)
                .HasDefaultValueSql("'25'")
                .HasColumnType("int")
                .HasColumnName("buddyCapacity");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("createdate");
            entity.Property(e => e.DataString)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("dataString");
            entity.Property(e => e.Dex)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int")
                .HasColumnName("dex");
            entity.Property(e => e.DojoPoints)
                .HasColumnType("int")
                .HasColumnName("dojoPoints");
            entity.Property(e => e.Equipslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType("int")
                .HasColumnName("equipslots");
            entity.Property(e => e.Etcslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType("int")
                .HasColumnName("etcslots");
            entity.Property(e => e.Exp)
                .HasColumnType("int")
                .HasColumnName("exp");
            entity.Property(e => e.Face)
                .HasColumnType("int")
                .HasColumnName("face");
            entity.Property(e => e.Fame)
                .HasColumnType("int")
                .HasColumnName("fame");
            entity.Property(e => e.FamilyId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("int")
                .HasColumnName("familyId");
            entity.Property(e => e.FinishedDojoTutorial)
                .HasColumnType("tinyint(1)")
                .HasColumnName("finishedDojoTutorial");
            entity.Property(e => e.Fquest)
                .HasColumnType("int")
                .HasColumnName("fquest");
            entity.Property(e => e.Gachaexp)
                .HasColumnType("int")
                .HasColumnName("gachaexp");
            entity.Property(e => e.Gender)
                .HasColumnType("int")
                .HasColumnName("gender");
            entity.Property(e => e.GuildId)
                .HasColumnType("int")
                .HasColumnName("guildid");
            entity.Property(e => e.GuildRank)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int")
                .HasColumnName("guildrank");
            entity.Property(e => e.Hair)
                .HasColumnType("int")
                .HasColumnName("hair");
            entity.Property(e => e.Hp)
                .HasDefaultValueSql("'50'")
                .HasColumnType("int")
                .HasColumnName("hp");
            entity.Property(e => e.HpMpUsed)
                .HasColumnType("int")
                .HasColumnName("hpMpUsed");
            entity.Property(e => e.Int)
                .HasDefaultValueSql("'4'")
                .HasColumnType("int")
                .HasColumnName("int");
            entity.Property(e => e.Jailexpire)
                .HasColumnType("bigint")
                .HasColumnName("jailexpire");
            entity.Property(e => e.JobId)
                .HasColumnType("int")
                .HasColumnName("job");
            entity.Property(e => e.JobRank)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("jobRank");
            entity.Property(e => e.JobRankMove)
                .HasColumnType("int")
                .HasColumnName("jobRankMove");
            entity.Property(e => e.LastDojoStage)
                .HasColumnType("int")
                .HasColumnName("lastDojoStage");
            entity.Property(e => e.LastExpGainTime)
                .HasDefaultValueSql("'2015-01-01 05:00:00'")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("lastExpGainTime");
            entity.Property(e => e.LastLogoutTime)
                .HasDefaultValueSql("'2015-01-01 05:00:00'")
                .HasColumnType("timestamp")
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero))
                .HasColumnName("lastLogoutTime");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("level");
            entity.Property(e => e.Luk)
                .HasDefaultValueSql("'4'")
                .HasColumnType("int")
                .HasColumnName("luk");
            entity.Property(e => e.Map)
                .HasColumnType("int")
                .HasColumnName("map");
            entity.Property(e => e.Matchcardlosses)
                .HasColumnType("int")
                .HasColumnName("matchcardlosses");
            entity.Property(e => e.Matchcardties)
                .HasColumnType("int")
                .HasColumnName("matchcardties");
            entity.Property(e => e.Matchcardwins)
                .HasColumnType("int")
                .HasColumnName("matchcardwins");
            entity.Property(e => e.Maxhp)
                .HasDefaultValueSql("'50'")
                .HasColumnType("int")
                .HasColumnName("maxhp");
            entity.Property(e => e.Maxmp)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int")
                .HasColumnName("maxmp");
            entity.Property(e => e.Meso)
                .HasColumnType("int")
                .HasColumnName("meso");
            entity.Property(e => e.Monsterbookcover)
                .HasColumnType("int")
                .HasColumnName("monsterbookcover");
            entity.Property(e => e.MountExp)
                .HasColumnType("int")
                .HasColumnName("mountexp");
            entity.Property(e => e.MountLevel)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("mountlevel");
            entity.Property(e => e.Mounttiredness)
                .HasColumnType("int")
                .HasColumnName("mounttiredness");
            entity.Property(e => e.Mp)
                .HasDefaultValueSql("'5'")
                .HasColumnType("int")
                .HasColumnName("mp");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Omoklosses)
                .HasColumnType("int")
                .HasColumnName("omoklosses");
            entity.Property(e => e.Omokties)
                .HasColumnType("int")
                .HasColumnName("omokties");
            entity.Property(e => e.Omokwins)
                .HasColumnType("int")
                .HasColumnName("omokwins");
            entity.Property(e => e.PartySearch)
                .IsRequired()
                .HasDefaultValue(true)
                .HasSentinel(true)
                .HasColumnName("partySearch");
            entity.Property(e => e.Pqpoints)
                .HasColumnType("int")
                .HasColumnName("PQPoints");
            entity.Property(e => e.Rank)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("rank");
            entity.Property(e => e.RankMove)
                .HasColumnType("int")
                .HasColumnName("rankMove");
            entity.Property(e => e.Reborns)
                .HasColumnType("int")
                .HasColumnName("reborns");
            entity.Property(e => e.Setupslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType("int")
                .HasColumnName("setupslots");
            entity.Property(e => e.Skincolor)
                .HasColumnType("int")
                .HasColumnName("skincolor");
            entity.Property(e => e.Sp)
                .HasMaxLength(128)
                .HasDefaultValueSql("'0,0,0,0,0,0,0,0,0,0'")
                .HasColumnName("sp");
            entity.Property(e => e.Spawnpoint)
                .HasColumnType("int")
                .HasColumnName("spawnpoint");
            entity.Property(e => e.Str)
                .HasDefaultValueSql("'12'")
                .HasColumnType("int")
                .HasColumnName("str");
            entity.Property(e => e.SummonValue)
                .HasColumnType("int")
                .HasColumnName("summonValue");
            entity.Property(e => e.Useslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType("int")
                .HasColumnName("useslots");
            entity.Property(e => e.VanquisherKills)
                .HasColumnType("int")
                .HasColumnName("vanquisherKills");
            entity.Property(e => e.VanquisherStage)
                .HasColumnType("int")
                .HasColumnName("vanquisherStage");
            entity.Property(e => e.World)
                .HasColumnType("int")
                .HasColumnName("world");
        });
    }

    private void ConfigInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventoryitem>(entity =>
        {
            entity.HasKey(e => e.Inventoryitemid).HasName("PRIMARY");

            entity.ToTable("inventoryitems");

            entity.HasIndex(e => e.Characterid, "idx_inv_charId");

            entity.Property(e => e.Inventoryitemid)
                .HasColumnName("inventoryitemid");
            entity.Property(e => e.Accountid)
                .HasColumnType("int")
                .HasColumnName("accountid");
            entity.Property(e => e.Characterid)
                .HasColumnType("int")
                .HasColumnName("characterid");
            entity.Property(e => e.Expiration)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("expiration");
            entity.Property(e => e.Flag)
                .HasColumnType("int")
                .HasColumnName("flag");
            entity.Property(e => e.GiftFrom)
                .HasMaxLength(26)
                .HasColumnName("giftFrom");
            entity.Property(e => e.Inventorytype)
                .HasColumnType("int")
                .HasColumnName("inventorytype");
            entity.Property(e => e.Itemid)
                .HasColumnType("int")
                .HasColumnName("itemid");
            entity.Property(e => e.Owner)
                .HasColumnType("tinytext")
                .HasColumnName("owner");
            entity.Property(e => e.Petid)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("petid");
            entity.Property(e => e.Position)
                .HasColumnType("int")
                .HasColumnName("position");
            entity.Property(e => e.Quantity)
                .HasColumnType("int")
                .HasColumnName("quantity");
            entity.Property(e => e.Type)
                .HasColumnType("tinyint")
                .HasColumnName("type");
        });

        modelBuilder.Entity<Inventoryequipment>(entity =>
        {
            entity.HasKey(e => e.Inventoryequipmentid).HasName("PRIMARY");

            entity.ToTable("inventoryequipment");

            entity.HasIndex(e => e.Inventoryitemid, "INVENTORYITEMID");

            entity.Property(e => e.Inventoryequipmentid)
                .HasColumnName("inventoryequipmentid");
            entity.Property(e => e.Acc)
                .HasColumnType("int")
                .HasColumnName("acc");
            entity.Property(e => e.Avoid)
                .HasColumnType("int")
                .HasColumnName("avoid");
            entity.Property(e => e.Dex)
                .HasColumnType("int")
                .HasColumnName("dex");
            entity.Property(e => e.Hands)
                .HasColumnType("int")
                .HasColumnName("hands");
            entity.Property(e => e.Hp)
                .HasColumnType("int")
                .HasColumnName("hp");
            entity.Property(e => e.Int)
                .HasColumnType("int")
                .HasColumnName("int");
            entity.Property(e => e.Inventoryitemid)
                .HasColumnType("int")
                .HasColumnName("inventoryitemid");
            entity.Property(e => e.Itemexp)
                .HasColumnType("int")
                .HasColumnName("itemexp");
            entity.Property(e => e.Itemlevel)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int")
                .HasColumnName("itemlevel");
            entity.Property(e => e.Jump)
                .HasColumnType("int")
                .HasColumnName("jump");
            entity.Property(e => e.Level)
                .HasColumnType("int")
                .HasColumnName("level");
            entity.Property(e => e.Locked)
                .HasColumnType("int")
                .HasColumnName("locked");
            entity.Property(e => e.Luk)
                .HasColumnType("int")
                .HasColumnName("luk");
            entity.Property(e => e.Matk)
                .HasColumnType("int")
                .HasColumnName("matk");
            entity.Property(e => e.Mdef)
                .HasColumnType("int")
                .HasColumnName("mdef");
            entity.Property(e => e.Mp)
                .HasColumnType("int")
                .HasColumnName("mp");
            entity.Property(e => e.RingId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType("bigint")
                .HasColumnName("ringid");
            entity.Property(e => e.Speed)
                .HasColumnType("int")
                .HasColumnName("speed");
            entity.Property(e => e.Str)
                .HasColumnType("int")
                .HasColumnName("str");
            entity.Property(e => e.Upgradeslots)
                .HasColumnType("int")
                .HasColumnName("upgradeslots");
            entity.Property(e => e.Vicious)
                .HasColumnType("int")
                .HasColumnName("vicious");
            entity.Property(e => e.Watk)
                .HasColumnType("int")
                .HasColumnName("watk");
            entity.Property(e => e.Wdef)
                .HasColumnType("int")
                .HasColumnName("wdef");
        });
    }

}

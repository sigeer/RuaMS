using Application.Core.EF;
using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Gachapons;
using Application.Core.EF.Utils;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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


    public virtual DbSet<BbsReplyEntity> BbsReplies { get; set; }

    public virtual DbSet<BbsThreadEntity> BbsThreads { get; set; }

    public virtual DbSet<BossLogEntity> BossLogs { get; set; }


    public virtual DbSet<CharacterEntity> Characters { get; set; }


    public virtual DbSet<DropDataGlobalEntity> DropDataGlobals { get; set; }

    public virtual DbSet<DropDataEntity> DropData { get; set; }

    public virtual DbSet<DueyPackageEntity> Dueypackages { get; set; }



    public virtual DbSet<FamilyCharacterEntity> FamilyCharacters { get; set; }

    public virtual DbSet<FamilyEntitlementEntity> FamilyEntitlements { get; set; }

    public virtual DbSet<FredstorageEntity> Fredstorages { get; set; }

    public virtual DbSet<GiftEntity> Gifts { get; set; }

    public virtual DbSet<GuildEntity> Guilds { get; set; }

    public virtual DbSet<Hwidaccount> Hwidaccounts { get; set; }

    public virtual DbSet<HwidbanEntity> Hwidbans { get; set; }


    public virtual DbSet<IpbanEntity> Ipbans { get; set; }


    public virtual DbSet<MacbanEntity> Macbans { get; set; }



    public virtual DbSet<MarriageEntity> Marriages { get; set; }


    public virtual DbSet<MonsterCardDataEntity> Monstercarddata { get; set; }

    public virtual DbSet<MtsCart> MtsCarts { get; set; }

    public virtual DbSet<MtsItemEntity> MtsItems { get; set; }

    public virtual DbSet<Namechange> Namechanges { get; set; }

    public virtual DbSet<NewYearCardEntity> Newyears { get; set; }

    public virtual DbSet<NoteEntity> Notes { get; set; }

    public virtual DbSet<RewardEntity> RewardCodes { get; set; }

    public virtual DbSet<RewardItemEntity> RewardItems { get; set; }
    public virtual DbSet<RewardRecordEntity> RewardRecords { get; set; }

    public virtual DbSet<PlayerNpcEntity> Playernpcs { get; set; }

    public virtual DbSet<PlayerNpcsEquipEntity> PlayernpcsEquips { get; set; }

    public virtual DbSet<PlayernpcsField> PlayernpcsFields { get; set; }

    public virtual DbSet<PlifeEntity> Plives { get; set; }


    public virtual DbSet<ReactorDropEntity> Reactordrops { get; set; }

    public virtual DbSet<ReportEntity> Reports { get; set; }


    public virtual DbSet<RingEntity> Rings { get; set; }



    public virtual DbSet<ShopEntity> Shops { get; set; }

    public virtual DbSet<ShopItemEntity> Shopitems { get; set; }


    public virtual DbSet<SpecialCashItemEntity> Specialcashitems { get; set; }

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging(true);
    }

    /// <summary>
    /// 当前使用的数据库提供程序，由 EF 已解析的 <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.ProviderName"/> 推断。
    /// </summary>
    protected DbProvider CurrentProvider => Database.ProviderName switch
    {
        var name when name != null && name.Contains("npgsql", StringComparison.OrdinalIgnoreCase) => DbProvider.PostgreSql,
        _ => DbProvider.Sqlite,
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var unixConverter = new DateTimeOffsetToUnixMillisecondsConverter();
        var unixNullableConverter = new DateTimeOffsetToUnixMillisecondsNullableConverter();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(unixConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(unixNullableConverter);
                }
            }
        }

        var provider = CurrentProvider;
        ConfigAccountCharacter(modelBuilder, provider);

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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("postercid");
            entity.Property(e => e.Threadid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("threadid");
            entity.Property(e => e.Timestamp)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<BbsThreadEntity>(entity =>
        {
            entity.HasKey(e => e.Threadid).HasName("PRIMARY");

            entity.ToTable("bbs_threads");

            entity.Property(e => e.Threadid)
                .HasColumnName("threadid");
            entity.Property(e => e.Guildid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("guildid");
            entity.Property(e => e.Icon)
                .HasColumnType(DbColumnTypes.SmallInt(provider))
                .HasColumnName("icon");
            entity.Property(e => e.Localthreadid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("localthreadid");
            entity.Property(e => e.Name)
                .HasMaxLength(26)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Postercid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("postercid");
            entity.Property(e => e.Replycount)
                .HasColumnType(DbColumnTypes.SmallInt(provider))
                .HasColumnName("replycount");
            entity.Property(e => e.Startpost)
                .HasColumnType(DbColumnTypes.Text(provider))
                .HasColumnName("startpost");
            entity.Property(e => e.Timestamp)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("timestamp");
        });


        modelBuilder.Entity<BossLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("boss_log");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Time);
            entity.Property(e => e.BossType)
                .HasColumnType(DbColumnTypes.Varchar(provider, 20));
            entity.Property(e => e.CharacterId)
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.Flag)
                .HasColumnType(DbColumnTypes.Int(provider));
        });


        modelBuilder.Entity<DropDataGlobalEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sys_drop_data_global");

            entity.HasIndex(e => e.Continent, "mobid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chance)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("chance");
            entity.Property(e => e.Comments)
                .HasMaxLength(45)
                .HasColumnName("comments");
            entity.Property(e => e.Continent)
                .IsRequired()
                .HasDefaultValueSql("'-1'")
                .HasColumnName("continent")
                .HasColumnType(DbColumnTypes.TinyInt(provider));
            entity.Property(e => e.Itemid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.MaximumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("maximum_quantity");
            entity.Property(e => e.MinimumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("minimum_quantity");
            entity.Property(e => e.Questid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("questid");
        });

        modelBuilder.Entity<DropDataEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sys_drop_data");

            entity.HasIndex(e => new { e.Dropperid, e.Itemid }, "dropperid").IsUnique();

            entity.HasIndex(e => new { e.Dropperid, e.Itemid }, "dropperid_2");

            entity.HasIndex(e => e.Dropperid, "mobid");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chance)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("chance");
            entity.Property(e => e.Dropperid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("dropperid");
            entity.Property(e => e.Itemid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.MaximumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("maximum_quantity");
            entity.Property(e => e.MinimumQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("minimum_quantity");
            entity.Property(e => e.Questid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("questid");
        });

        modelBuilder.Entity<ReactorDropEntity>(entity =>
        {
            entity.HasKey(e => e.Reactordropid).HasName("PRIMARY");

            entity.ToTable("sys_reactordrops");

            entity.HasIndex(e => e.Reactorid, "reactorid");

            entity.Property(e => e.Reactordropid)
                .HasColumnName("reactordropid");
            entity.Property(e => e.Chance)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("chance");
            entity.Property(e => e.Itemid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.Questid)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("questid");
            entity.Property(e => e.Reactorid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("reactorid");
        });


        modelBuilder.Entity<DueyPackageEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dueypackages");

            entity.Property(e => e.Id);
            entity.Property(e => e.HasNotified)
                .HasDefaultValue(false)
                .HasColumnType(DbColumnTypes.Boolean(provider));
            entity.Property(e => e.Mesos)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.Message).HasMaxLength(200);
            entity.Property(e => e.ReceiverId).HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.SenderId).HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.CreateTime);
            entity.Property(e => e.Type)
                .HasDefaultValue(false)
                .HasColumnType(DbColumnTypes.Boolean(provider));
            entity.Property(e => e.ItemBlob)
                .HasColumnType(DbColumnTypes.Blob(provider));
        });



        modelBuilder.Entity<FamilyCharacterEntity>(entity =>
        {
            entity.HasKey(e => e.Cid).HasName("PRIMARY");

            entity.ToTable("family_character");

            entity.HasIndex(e => new { e.Cid, e.Familyid }, "cid");

            entity.Property(e => e.Cid)
                .HasColumnName("cid");
            entity.Property(e => e.Familyid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("familyid");
            entity.Property(e => e.Lastresettime)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("lastresettime");
            entity.Property(e => e.Precepts)
                .HasMaxLength(200)
                .HasColumnName("precepts");
            entity.Property(e => e.Reptosenior)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("reptosenior");
            entity.Property(e => e.Reputation)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("reputation");
            entity.Property(e => e.Seniorid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("seniorid");
            entity.Property(e => e.Todaysrep)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("todaysrep");
            entity.Property(e => e.Totalreputation)
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("charid");
            entity.Property(e => e.Entitlementid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("entitlementid");
            entity.Property(e => e.Timestamp)
                .HasColumnType(DbColumnTypes.BigInt(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("cid");
            entity.Property(e => e.Daynotes)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("daynotes");
            entity.Property(e => e.Meso)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("meso");
            entity.Property(e => e.ItemMeso)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("itemMeso");
            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp");
            entity.Property(e => e.ItemsBlob)
                .HasColumnType(DbColumnTypes.Blob(provider));
        });

        modelBuilder.Entity<GiftEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("gifts");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.FromId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fromId");
            entity.Property(e => e.Message)
                .HasColumnType(DbColumnTypes.TinyText(provider))
                .HasColumnName("message");
            entity.Property(e => e.RingSourceId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("ringSourceId");
            entity.Property(e => e.Sn)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("sn");
            entity.Property(e => e.ToId)
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("allianceId");
            entity.Property(e => e.Capacity)
                .HasDefaultValueSql("'10'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("capacity");
            entity.Property(e => e.GP)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("GP");
            entity.Property(e => e.Leader)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("leader");
            entity.Property(e => e.Logo)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("logo");
            entity.Property(e => e.LogoBg)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("logoBG");
            entity.Property(e => e.LogoBgColor)
                .HasColumnType(DbColumnTypes.SmallInt(provider))
                .HasColumnName("logoBGColor");
            entity.Property(e => e.LogoColor)
                .HasColumnType(DbColumnTypes.SmallInt(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnName("expiresat");
            entity.Property(e => e.Relevance)
                .HasColumnType(DbColumnTypes.TinyInt(provider))
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



        modelBuilder.Entity<MarriageEntity>(entity =>
        {
            entity.HasKey(e => e.Marriageid).HasName("PRIMARY");

            entity.ToTable("marriages");

            entity.Property(e => e.Marriageid)
                .HasColumnName("marriageid");
            entity.Property(e => e.Husbandid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("husbandid");
            entity.Property(e => e.Wifeid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("wifeid");
        });



        modelBuilder.Entity<MonsterCardDataEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sys_monstercarddata");

            entity.HasIndex(e => e.Id, "id").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cardid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("cardid");
            entity.Property(e => e.Mobid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mobid");
        });

        modelBuilder.Entity<MtsCart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mts_cart");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("cid");
            entity.Property(e => e.Itemid)
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("acc");
            entity.Property(e => e.Avoid)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("avoid");
            entity.Property(e => e.BidIncre)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("bid_incre");
            entity.Property(e => e.BuyNow)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("buy_now");
            entity.Property(e => e.Dex)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("dex");
            entity.Property(e => e.Expiration)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("expiration");
            entity.Property(e => e.Flag)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("flag");
            entity.Property(e => e.GiftFrom)
                .HasMaxLength(26)
                .HasColumnName("giftFrom");
            entity.Property(e => e.Hands)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hands");
            entity.Property(e => e.Hp)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hp");
            entity.Property(e => e.Int)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("int");
            entity.Property(e => e.Isequip)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("isequip");
            entity.Property(e => e.Itemexp)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemexp");
            entity.Property(e => e.Itemid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.Itemlevel)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemlevel");
            entity.Property(e => e.Jump)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("jump");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("level");
            entity.Property(e => e.Locked)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("locked");
            entity.Property(e => e.Luk)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("luk");
            entity.Property(e => e.Matk)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("matk");
            entity.Property(e => e.Mdef)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mdef");
            entity.Property(e => e.Mp)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mp");
            entity.Property(e => e.Owner)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .HasColumnName("owner");
            entity.Property(e => e.Position)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("position");
            entity.Property(e => e.Price)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("quantity");
            entity.Property(e => e.Ringid)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("ringid");
            entity.Property(e => e.SellEnds)
                .HasMaxLength(16)
                .HasColumnName("sell_ends");
            entity.Property(e => e.Seller)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("seller");
            entity.Property(e => e.Sellername)
                .HasMaxLength(16)
                .HasColumnName("sellername");
            entity.Property(e => e.Speed)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("speed");
            entity.Property(e => e.Str)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("str");
            entity.Property(e => e.Tab)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("tab");
            entity.Property(e => e.Transfer)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("transfer");
            entity.Property(e => e.Type)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("type");
            entity.Property(e => e.Upgradeslots)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("upgradeslots");
            entity.Property(e => e.Vicious)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("vicious");
            entity.Property(e => e.Watk)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("watk");
            entity.Property(e => e.Wdef)
                .HasDefaultValueSql("'0'")
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("characterid");
            entity.Property(e => e.CompletionTime)
                .HasColumnName("completionTime");
            entity.Property(e => e.New)
                .HasMaxLength(13)
                .HasColumnName("new");
            entity.Property(e => e.Old)
                .HasMaxLength(13)
                .HasColumnName("old");
            entity.Property(e => e.RequestTime)
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
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("receiverid");
            entity.Property(e => e.SenderDiscard).HasColumnName("senderdiscard");
            entity.Property(e => e.SenderId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("senderid");
            entity.Property(e => e.TimeReceived)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("timereceived");
            entity.Property(e => e.TimeSent)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("timesent");
        });

        modelBuilder.Entity<NoteEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notes").HasQueryFilter(x => !x.Deleted);

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Deleted)
                .HasColumnName("deleted");
            entity.Property(e => e.Fame)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fame");
            entity.Property(e => e.FromId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fromId");
            entity.Property(e => e.Message)
                .HasColumnType(DbColumnTypes.Text(provider))
                .HasColumnName("message");
            entity.Property(e => e.Timestamp)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("timestamp");
            entity.Property(e => e.ToId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("toId");
        });

        modelBuilder.Entity<RewardEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ex_reward_codes");

            entity.HasIndex(e => e.Code, "idx_cdk_codes_code")
                .IsUnique();

            entity.Property(e => e.Id);
            entity.Property(e => e.Title)
                .HasMaxLength(50);
            entity.Property(e => e.Code)
                .HasMaxLength(17);
            entity.Property(e => e.StartTime);
            entity.Property(e => e.MaxCount)
                .HasColumnType(DbColumnTypes.Int(provider));
        });

        modelBuilder.Entity<RewardItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ex_reward_items");

            entity.HasIndex(e => e.CodeId, "idx_cdk_items_code");

            entity.Property(e => e.Id);
            entity.Property(e => e.CodeId)
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.ItemId)
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider));
        });

        modelBuilder.Entity<RewardRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ex_reward_records");

            entity.HasIndex(e => e.CodeId, "idx_code_records");

            entity.Property(e => e.Id);
            entity.Property(e => e.CodeId)
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.RecipientId)
                .HasColumnType(DbColumnTypes.Int(provider));
            entity.Property(e => e.RecipientTime);
        });



        modelBuilder.Entity<PlayerNpcEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cy)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("cy");
            entity.Property(e => e.Dir)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("dir");
            entity.Property(e => e.Face)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("face");
            entity.Property(e => e.Fh)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fh");
            entity.Property(e => e.Gender)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("gender");
            entity.Property(e => e.Hair)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hair");
            entity.Property(e => e.Job)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("job");
            entity.Property(e => e.Map)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("map");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasColumnName("name");
            entity.Property(e => e.Overallrank)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("overallrank");
            entity.Property(e => e.Rx0)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rx0");
            entity.Property(e => e.Rx1)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rx1");
            entity.Property(e => e.Scriptid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("scriptid");
            entity.Property(e => e.Skin)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("skin");
            entity.Property(e => e.World)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("world");
            entity.Property(e => e.Worldjobrank)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("worldjobrank");
            entity.Property(e => e.Worldrank)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("worldrank");
            entity.Property(e => e.X)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("x");
        });

        modelBuilder.Entity<PlayerNpcsEquipEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs_equip");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Equipid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("equipid");
            entity.Property(e => e.Equippos)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("equippos");
            entity.Property(e => e.Npcid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("npcid");
            entity.Property(e => e.Type)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("type");
        });

        modelBuilder.Entity<PlayernpcsField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("playernpcs_field");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Map)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("map");
            entity.Property(e => e.Podium)
                .HasColumnType(DbColumnTypes.SmallInt(provider))
                .HasColumnName("podium");
            entity.Property(e => e.Step).HasColumnName("step").HasColumnType(DbColumnTypes.TinyInt(provider)).HasDefaultValueSql("'0'");
            entity.Property(e => e.World)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("world");
        });

        modelBuilder.Entity<PlifeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plife");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Cy)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("cy");
            entity.Property(e => e.F)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("f");
            entity.Property(e => e.Fh)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fh");
            entity.Property(e => e.Hide)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hide");
            entity.Property(e => e.Life)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("life");
            entity.Property(e => e.Map)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("map");
            entity.Property(e => e.Mobtime)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mobtime");
            entity.Property(e => e.Rx0)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rx0");
            entity.Property(e => e.Rx1)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rx1");
            entity.Property(e => e.Team)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("team");
            entity.Property(e => e.Type)
                .HasMaxLength(1)
                .HasDefaultValueSql("'n'")
                .HasColumnName("type");
            entity.Property(e => e.World)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("world");
            entity.Property(e => e.X)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("x");
            entity.Property(e => e.Y)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("y");
        });




        modelBuilder.Entity<ReportEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reports");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Chatlog)
                .HasColumnType(DbColumnTypes.Text(provider))
                .HasColumnName("chatlog");
            entity.Property(e => e.Description)
                .HasColumnType(DbColumnTypes.Text(provider))
                .HasColumnName("description");
            entity.Property(e => e.Reason)
                .HasColumnType(DbColumnTypes.TinyInt(provider))
                .HasColumnName("reason");
            entity.Property(e => e.ReporterId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("reporterid");
            entity.Property(e => e.ReportTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnName("reporttime");
            entity.Property(e => e.VictimId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("victimid");
        });


        modelBuilder.Entity<RingEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rings");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ItemId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.CharacterId1)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("characterId1");
            entity.Property(e => e.RingId1)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("ringId1");
            entity.Property(e => e.CharacterId2)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("characterId2");
            entity.Property(e => e.RingId2)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("ringId2");
        });


        modelBuilder.Entity<ShopEntity>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PRIMARY");

            entity.ToTable("sys_shops");

            entity.Property(e => e.ShopId)
                .HasColumnName("shopid");
            entity.Property(e => e.NpcId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("npcid");
        });

        modelBuilder.Entity<ShopItemEntity>(entity =>
        {
            entity.HasKey(e => e.Shopitemid).HasName("PRIMARY");

            entity.ToTable("sys_shopitems");

            entity.Property(e => e.Shopitemid)
                .HasColumnName("shopitemid");
            entity.Property(e => e.ItemId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("itemid");
            entity.Property(e => e.Pitch)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("pitch");
            entity.Property(e => e.Position)
                .HasComment("sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("position");
            entity.Property(e => e.Price)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("price");
            entity.Property(e => e.Shopid)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("shopid");
        });


        modelBuilder.Entity<SpecialCashItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sys_specialcashitems");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Info)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("info");
            entity.Property(e => e.Modifier)
                .HasComment("1024 is add/remove")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("modifier");
            entity.Property(e => e.Sn)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("sn");
        });



        modelBuilder.Entity<ExpLogRecord>(entity =>
        {
            entity.ToTable("characterexplogs");
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.WorldExpRate)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("world_exp_rate");
            entity.Property(e => e.ExpCoupon)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("exp_coupon");
            entity.Property(e => e.GainedExp)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("gained_exp");
            entity.Property(e => e.CurrentExp)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("current_exp");
            entity.Property(e => e.ExpGainTime)
                .HasColumnName("exp_gain_time");
            entity.Property(e => e.CharId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("charid");
        });

        modelBuilder.Entity<GachaponPoolEntity>(entity =>
        {
            entity.ToTable("sys_gachapon_pool");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Name).HasColumnType(DbColumnTypes.Varchar(provider, 50)).IsRequired().HasDefaultValueSql("''");
        });

        modelBuilder.Entity<GachaponPoolItemEntity>(entity =>
        {
            entity.ToTable("sys_gachapon_pool_item");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });


        modelBuilder.Entity<GachaponPoolLevelChanceEntity>(entity =>
        {
            entity.ToTable("sys_gachapon_pool_level_chance");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });


        OnModelCreatingPartial(modelBuilder);

        // SQLite 允许所有表的主键都叫 PRIMARY，PostgreSQL 的索引名在 schema 内必须唯一
        if (provider == DbProvider.PostgreSql)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var key = entityType.FindPrimaryKey();
                if (key != null && key.GetName() == "PRIMARY")
                {
                    key.SetName($"{entityType.GetTableName()}_pkey");
                }
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private void ConfigAccountCharacter(ModelBuilder modelBuilder, DbProvider provider)
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
                .HasColumnType(DbColumnTypes.Date(provider))
                .HasColumnName("birthday");
            entity.Property(e => e.Characterslots)
                .HasDefaultValueSql("'3'")
                .HasColumnType(DbColumnTypes.TinyInt(provider))
                .HasColumnName("characterslots");
            entity.Property(e => e.Createdat)
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'10'")
                .HasColumnType(DbColumnTypes.TinyInt(provider))
                .HasColumnName("gender");
            entity.Property(e => e.MaplePoint)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("maplePoint");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Nick)
                .HasMaxLength(20)
                .HasColumnName("nick");
            entity.Property(e => e.NxCredit)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("nxCredit");
            entity.Property(e => e.NxPrepaid)
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnType(DbColumnTypes.TinyInt(provider))
                .HasDefaultValueSql("'0'")
                .HasColumnName("gmlevel");
            entity.Property(e => e.Tos).HasColumnName("tos");
            entity.Property(e => e.Blob)
                .HasColumnType(DbColumnTypes.Blob(provider));

            // 账号名、邮箱与密码都忽略大小写匹配：
            // 账号名/邮箱大小写不同视为同一账号；密码存的是十六进制哈希，
            // 早期数据存在大小写不一致的情况，用 NOCASE 保持兼容。
            var ciCollation = DbColumnTypes.CaseInsensitiveCollation(provider);
            entity.Property(e => e.Email)
                .UseCollation(ciCollation);
            entity.Property(e => e.Name)
                .UseCollation(ciCollation);
            entity.Property(e => e.Password)
                .UseCollation(ciCollation);
        });

        modelBuilder.Entity<AccountBindingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("account_bindings");

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.Property(e => e.Id)
                .HasColumnName("Id");
            entity.Property(e => e.AccountId)
                .HasColumnType(DbColumnTypes.Int(provider))
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
                .HasColumnName("LastActiveTime");
        });

        modelBuilder.Entity<AccountBanEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("account_ban");

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.Property(e => e.Id)
                .HasColumnName("Id");
            entity.Property(e => e.AccountId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("AccountId");

            entity.Property(e => e.Reason)
                .HasColumnType(DbColumnTypes.TinyInt(provider))
                .HasColumnName("Reason");

            entity.Property(e => e.ReasonDescription)
                .HasColumnType(DbColumnTypes.Text(provider))
                .HasColumnName("ReasonDescription");

            entity.Property(e => e.StartTime)
                .HasColumnName("StartTime");

            entity.Property(e => e.EndTime)
                .HasColumnName("EndTime");
        });

        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("characters").HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.AccountId, "accountid");

            entity.HasIndex(e => new { e.Id, e.AccountId, e.Name }, "id_2");

            entity.HasIndex(e => new { e.Level, e.Exp }, "ranking1");

            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.AccountId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("accountid");
            entity.Property(e => e.AllianceRank)
                .HasDefaultValueSql("'5'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("allianceRank");
            entity.Property(e => e.Ap)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("ap");
            entity.Property(e => e.AriantPoints)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("ariantPoints");
            entity.Property(e => e.BuddyCapacity)
                .HasDefaultValueSql("'25'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("buddyCapacity");
            entity.Property(e => e.CreateDate)
                .HasColumnName("createdate");
            entity.Property(e => e.DataString)
                .HasMaxLength(64)
                .HasDefaultValueSql("''")
                .HasColumnName("dataString");
            entity.Property(e => e.Dex)
                .HasDefaultValueSql("'5'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("dex");
            entity.Property(e => e.DojoPoints)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("dojoPoints");
            entity.Property(e => e.Equipslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("equipslots");
            entity.Property(e => e.Etcslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("etcslots");
            entity.Property(e => e.Exp)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("exp");
            entity.Property(e => e.Face)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("face");
            entity.Property(e => e.Fame)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fame");
            entity.Property(e => e.FamilyId)
                .HasDefaultValueSql("'-1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("familyId");
            entity.Property(e => e.FinishedDojoTutorial)
                .HasColumnType(DbColumnTypes.Boolean(provider))
                .HasColumnName("finishedDojoTutorial");
            entity.Property(e => e.Fquest)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("fquest");
            entity.Property(e => e.Gachaexp)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("gachaexp");
            entity.Property(e => e.Gender)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("gender");
            entity.Property(e => e.GuildId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("guildid");
            entity.Property(e => e.GuildRank)
                .HasDefaultValueSql("'5'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("guildrank");
            entity.Property(e => e.Hair)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hair");
            entity.Property(e => e.Hp)
                .HasDefaultValueSql("'50'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hp");
            entity.Property(e => e.HpMpUsed)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("hpMpUsed");
            entity.Property(e => e.Int)
                .HasDefaultValueSql("'4'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("int");
            entity.Property(e => e.Jailexpire)
                .HasColumnType(DbColumnTypes.BigInt(provider))
                .HasColumnName("jailexpire");
            entity.Property(e => e.JobId)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("job");
            entity.Property(e => e.JobRank)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("jobRank");
            entity.Property(e => e.JobRankMove)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("jobRankMove");
            entity.Property(e => e.LastDojoStage)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("lastDojoStage");
            entity.Property(e => e.LastExpGainTime)
                .HasColumnName("lastExpGainTime");
            entity.Property(e => e.LastLogoutTime)
                .HasColumnName("lastLogoutTime");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("level");
            entity.Property(e => e.Luk)
                .HasDefaultValueSql("'4'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("luk");
            entity.Property(e => e.Map)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("map");
            entity.Property(e => e.Matchcardlosses)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("matchcardlosses");
            entity.Property(e => e.Matchcardties)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("matchcardties");
            entity.Property(e => e.Matchcardwins)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("matchcardwins");
            entity.Property(e => e.Maxhp)
                .HasDefaultValueSql("'50'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("maxhp");
            entity.Property(e => e.Maxmp)
                .HasDefaultValueSql("'5'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("maxmp");
            entity.Property(e => e.Meso)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("meso");
            entity.Property(e => e.Monsterbookcover)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("monsterbookcover");
            entity.Property(e => e.MountExp)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mountexp");
            entity.Property(e => e.MountLevel)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mountlevel");
            entity.Property(e => e.Mounttiredness)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mounttiredness");
            entity.Property(e => e.Mp)
                .HasDefaultValueSql("'5'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("mp");
            entity.Property(e => e.Name)
                .HasMaxLength(13)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Omoklosses)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("omoklosses");
            entity.Property(e => e.Omokties)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("omokties");
            entity.Property(e => e.Omokwins)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("omokwins");
            entity.Property(e => e.PartySearch)
                .IsRequired()
                .HasDefaultValue(true)
                .HasSentinel(true)
                .HasColumnName("partySearch");
            entity.Property(e => e.Pqpoints)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("PQPoints");
            entity.Property(e => e.Rank)
                .HasDefaultValueSql("'1'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rank");
            entity.Property(e => e.RankMove)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("rankMove");

            entity.Property(e => e.Setupslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("setupslots");
            entity.Property(e => e.Skincolor)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("skincolor");
            entity.Property(e => e.Sp)
                .HasMaxLength(128)
                .HasDefaultValueSql("'0,0,0,0,0,0,0,0,0,0'")
                .HasColumnName("sp");
            entity.Property(e => e.Spawnpoint)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("spawnpoint");
            entity.Property(e => e.Str)
                .HasDefaultValueSql("'12'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("str");
            entity.Property(e => e.SummonValue)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("summonValue");
            entity.Property(e => e.Useslots)
                .HasDefaultValueSql("'24'")
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("useslots");
            entity.Property(e => e.VanquisherKills)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("vanquisherKills");
            entity.Property(e => e.VanquisherStage)
                .HasColumnType(DbColumnTypes.Int(provider))
                .HasColumnName("vanquisherStage");
            entity.Property(e => e.IsDeleted)
                .HasColumnType(DbColumnTypes.Boolean(provider))
                .HasColumnName("IsDeleted");
            entity.Property(e => e.Blob)
                .HasColumnType(DbColumnTypes.Blob(provider));
        });
    }
}

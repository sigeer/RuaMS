using System;
using Application.Core.EF;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Application.Core.EF.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 账号名/邮箱/密码需要忽略大小写匹配。PostgreSQL 12+ 可以用 ICU 第 2 级比较规则
            // 声明非确定性排序规则，无需安装 citext 等扩展。
            // 注意：非确定性排序规则不支持 LIKE，模糊匹配需用 ILIKE。
            migrationBuilder.Sql(
                $"CREATE COLLATION IF NOT EXISTS {DbColumnTypes.PostgreSqlCaseInsensitiveCollation} " +
                "(provider = icu, locale = 'und-u-ks-level2', deterministic = false);");

            migrationBuilder.CreateTable(
                name: "account_ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<long>(type: "bigint", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: false),
                    BanLevel = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<short>(type: "smallint", nullable: false),
                    ReasonDescription = table.Column<string>(type: "text", nullable: false),
                    OperateAccountId = table.Column<int>(type: "integer", nullable: false),
                    AuditAccountId = table.Column<int>(type: "integer", nullable: false),
                    Canceled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_ban_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_bindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "''"),
                    MAC = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    HWID = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValueSql: "''"),
                    LastActiveTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_bindings_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false, defaultValueSql: "''", collation: "case_insensitive"),
                    password = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "''", collation: "case_insensitive"),
                    pin = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "''"),
                    pic = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    createdat = table.Column<long>(type: "bigint", nullable: false),
                    birthday = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "'2005-05-11'"),
                    nxCredit = table.Column<int>(type: "int", nullable: false),
                    maplePoint = table.Column<int>(type: "int", nullable: false),
                    nxPrepaid = table.Column<int>(type: "int", nullable: false),
                    characterslots = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'3'"),
                    gender = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'10'"),
                    tos = table.Column<bool>(type: "boolean", nullable: false),
                    gmlevel = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'0'"),
                    nick = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true, collation: "case_insensitive"),
                    Blob = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("accounts_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alliance",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "'2'"),
                    notice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "''"),
                    rank1 = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValueSql: "'Master'"),
                    rank2 = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3 = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank4 = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank5 = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("alliance_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bbs_replies",
                columns: table => new
                {
                    replyid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    threadid = table.Column<int>(type: "int", nullable: false),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("bbs_replies_pkey", x => x.replyid);
                });

            migrationBuilder.CreateTable(
                name: "bbs_threads",
                columns: table => new
                {
                    threadid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    icon = table.Column<short>(type: "smallint", nullable: false),
                    replycount = table.Column<short>(type: "smallint", nullable: false),
                    startpost = table.Column<string>(type: "text", nullable: false),
                    guildid = table.Column<int>(type: "int", nullable: false),
                    localthreadid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bbs_threads_pkey", x => x.threadid);
                });

            migrationBuilder.CreateTable(
                name: "boss_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    BossType = table.Column<string>(type: "varchar(20)", nullable: false),
                    Flag = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("boss_log_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "characterexplogs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charid = table.Column<int>(type: "int", nullable: false),
                    world_exp_rate = table.Column<int>(type: "int", nullable: false),
                    exp_coupon = table.Column<int>(type: "int", nullable: false),
                    gained_exp = table.Column<long>(type: "bigint", nullable: false),
                    current_exp = table.Column<int>(type: "int", nullable: false),
                    exp_gain_time = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("characterexplogs_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    level = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    exp = table.Column<int>(type: "int", nullable: false),
                    gachaexp = table.Column<int>(type: "int", nullable: false),
                    str = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'12'"),
                    dex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    luk = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'4'"),
                    @int = table.Column<int>(name: "int", type: "int", nullable: false, defaultValueSql: "'4'"),
                    hp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'50'"),
                    mp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    maxhp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'50'"),
                    maxmp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    meso = table.Column<int>(type: "int", nullable: false),
                    hpMpUsed = table.Column<int>(type: "int", nullable: false),
                    job = table.Column<int>(type: "int", nullable: false),
                    skincolor = table.Column<int>(type: "int", nullable: false),
                    gender = table.Column<int>(type: "int", nullable: false),
                    fame = table.Column<int>(type: "int", nullable: false),
                    fquest = table.Column<int>(type: "int", nullable: false),
                    hair = table.Column<int>(type: "int", nullable: false),
                    face = table.Column<int>(type: "int", nullable: false),
                    ap = table.Column<int>(type: "int", nullable: false),
                    sp = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValueSql: "'0,0,0,0,0,0,0,0,0,0'"),
                    map = table.Column<int>(type: "int", nullable: false),
                    spawnpoint = table.Column<int>(type: "int", nullable: false),
                    buddyCapacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'25'"),
                    createdate = table.Column<long>(type: "bigint", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    rankMove = table.Column<int>(type: "int", nullable: false),
                    jobRank = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    jobRankMove = table.Column<int>(type: "int", nullable: false),
                    guildid = table.Column<int>(type: "int", nullable: false),
                    guildrank = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    mountlevel = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    mountexp = table.Column<int>(type: "int", nullable: false),
                    mounttiredness = table.Column<int>(type: "int", nullable: false),
                    omokwins = table.Column<int>(type: "int", nullable: false),
                    omoklosses = table.Column<int>(type: "int", nullable: false),
                    omokties = table.Column<int>(type: "int", nullable: false),
                    matchcardwins = table.Column<int>(type: "int", nullable: false),
                    matchcardlosses = table.Column<int>(type: "int", nullable: false),
                    matchcardties = table.Column<int>(type: "int", nullable: false),
                    equipslots = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'24'"),
                    useslots = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'24'"),
                    setupslots = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'24'"),
                    etcslots = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'24'"),
                    familyId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    monsterbookcover = table.Column<int>(type: "int", nullable: false),
                    allianceRank = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    vanquisherStage = table.Column<int>(type: "int", nullable: false),
                    ariantPoints = table.Column<int>(type: "int", nullable: false),
                    dojoPoints = table.Column<int>(type: "int", nullable: false),
                    lastDojoStage = table.Column<int>(type: "int", nullable: false),
                    finishedDojoTutorial = table.Column<bool>(type: "boolean", nullable: false),
                    vanquisherKills = table.Column<int>(type: "int", nullable: false),
                    summonValue = table.Column<int>(type: "int", nullable: false),
                    PQPoints = table.Column<int>(type: "int", nullable: false),
                    dataString = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''"),
                    lastLogoutTime = table.Column<long>(type: "bigint", nullable: false),
                    lastExpGainTime = table.Column<long>(type: "bigint", nullable: false),
                    partySearch = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    jailexpire = table.Column<long>(type: "bigint", nullable: false),
                    HpAlert = table.Column<int>(type: "integer", nullable: false),
                    MpAlert = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Blob = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("characters_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dueypackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Mesos = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    CreateTime = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HasNotified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Type = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ItemBlob = table.Column<byte[]>(type: "bytea", nullable: true),
                    ClaimTime = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("dueypackages_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ex_reward_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    StartTime = table.Column<long>(type: "bigint", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: true),
                    MaxCount = table.Column<int>(type: "int", nullable: false),
                    AccountOnce = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_reward_codes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ex_reward_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_reward_items_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ex_reward_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    RecipientTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_reward_records_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "family_entitlement",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charid = table.Column<int>(type: "int", nullable: false),
                    entitlementid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("family_entitlement_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fredstorage",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cid = table.Column<int>(type: "int", nullable: false),
                    daynotes = table.Column<int>(type: "int", nullable: false),
                    meso = table.Column<int>(type: "int", nullable: false),
                    itemMeso = table.Column<long>(type: "bigint", nullable: false),
                    ItemsBlob = table.Column<byte[]>(type: "bytea", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("fredstorage_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    sn = table.Column<int>(type: "int", nullable: false),
                    ringSourceId = table.Column<int>(type: "int", nullable: false),
                    ClaimTime = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("gifts_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    guildid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    leader = table.Column<int>(type: "int", nullable: false),
                    GP = table.Column<int>(type: "int", nullable: false),
                    logo = table.Column<int>(type: "int", nullable: false),
                    logoColor = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    rank1title = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValueSql: "'Master'"),
                    rank2title = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3title = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank4title = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank5title = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    capacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'10'"),
                    logoBG = table.Column<int>(type: "int", nullable: false),
                    logoBGColor = table.Column<short>(type: "smallint", nullable: false),
                    notice = table.Column<string>(type: "character varying(101)", maxLength: 101, nullable: true),
                    signature = table.Column<int>(type: "int", nullable: false),
                    allianceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guilds_pkey", x => x.guildid);
                });

            migrationBuilder.CreateTable(
                name: "hwidaccounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "integer", nullable: false),
                    hwid = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    relevance = table.Column<short>(type: "smallint", nullable: false),
                    expiresat = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("hwidaccounts_pkey", x => new { x.accountid, x.hwid });
                });

            migrationBuilder.CreateTable(
                name: "hwidbans",
                columns: table => new
                {
                    hwidbanid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hwid = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    LinkedBanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("hwidbans_pkey", x => x.hwidbanid);
                });

            migrationBuilder.CreateTable(
                name: "ipbans",
                columns: table => new
                {
                    ipbanid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ip = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    aid = table.Column<int>(type: "integer", nullable: false),
                    LinkedBanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ipbans_pkey", x => x.ipbanid);
                });

            migrationBuilder.CreateTable(
                name: "macbans",
                columns: table => new
                {
                    macbanid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mac = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    aid = table.Column<int>(type: "integer", nullable: false),
                    LinkedBanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("macbans_pkey", x => x.macbanid);
                });

            migrationBuilder.CreateTable(
                name: "marriages",
                columns: table => new
                {
                    marriageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    husbandid = table.Column<int>(type: "int", nullable: false),
                    wifeid = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Time0 = table.Column<long>(type: "bigint", nullable: false),
                    Time1 = table.Column<long>(type: "bigint", nullable: true),
                    Time2 = table.Column<long>(type: "bigint", nullable: true),
                    RingSourceId = table.Column<int>(type: "integer", nullable: false),
                    EngagementItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("marriages_pkey", x => x.marriageid);
                });

            migrationBuilder.CreateTable(
                name: "mts_cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mts_cart_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mts_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tab = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    seller = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    bid_incre = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    buy_now = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    position = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    upgradeslots = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    level = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    itemlevel = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    itemexp = table.Column<int>(type: "int", nullable: false),
                    ringid = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    str = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    dex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    @int = table.Column<int>(name: "int", type: "int", nullable: false, defaultValueSql: "'0'"),
                    luk = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    hp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    mp = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    watk = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    matk = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    wdef = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    mdef = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    acc = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    avoid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    hands = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    speed = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    jump = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    locked = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    isequip = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    owner = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValueSql: "''"),
                    sellername = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    sell_ends = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    transfer = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    vicious = table.Column<int>(type: "int", nullable: false),
                    flag = table.Column<int>(type: "int", nullable: false),
                    expiration = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mts_items_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "namechanges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    old = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    @new = table.Column<string>(name: "new", type: "character varying(13)", maxLength: 13, nullable: false),
                    requestTime = table.Column<long>(type: "bigint", nullable: false),
                    completionTime = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("namechanges_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "newyear",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    senderid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    receiverid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    message = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, defaultValueSql: "''"),
                    senderdiscard = table.Column<bool>(type: "boolean", nullable: false),
                    receiverdiscard = table.Column<bool>(type: "boolean", nullable: false),
                    received = table.Column<bool>(type: "boolean", nullable: false),
                    timesent = table.Column<long>(type: "bigint", nullable: false),
                    timereceived = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("newyear_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    fame = table.Column<int>(type: "int", nullable: false),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notes_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playernpcs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    hair = table.Column<int>(type: "int", nullable: false),
                    face = table.Column<int>(type: "int", nullable: false),
                    skin = table.Column<int>(type: "int", nullable: false),
                    gender = table.Column<int>(type: "int", nullable: false),
                    x = table.Column<int>(type: "int", nullable: false),
                    cy = table.Column<int>(type: "int", nullable: false),
                    world = table.Column<int>(type: "int", nullable: false),
                    map = table.Column<int>(type: "int", nullable: false),
                    dir = table.Column<int>(type: "int", nullable: false),
                    scriptid = table.Column<int>(type: "int", nullable: false),
                    fh = table.Column<int>(type: "int", nullable: false),
                    rx0 = table.Column<int>(type: "int", nullable: false),
                    rx1 = table.Column<int>(type: "int", nullable: false),
                    worldrank = table.Column<int>(type: "int", nullable: false),
                    overallrank = table.Column<int>(type: "int", nullable: false),
                    worldjobrank = table.Column<int>(type: "int", nullable: false),
                    job = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("playernpcs_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playernpcs_equip",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    npcid = table.Column<int>(type: "int", nullable: false),
                    equipid = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    equippos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("playernpcs_equip_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playernpcs_field",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    world = table.Column<int>(type: "int", nullable: false),
                    map = table.Column<int>(type: "int", nullable: false),
                    step = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'0'"),
                    podium = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("playernpcs_field_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plife",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    world = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    map = table.Column<int>(type: "int", nullable: false),
                    life = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValueSql: "'n'"),
                    cy = table.Column<int>(type: "int", nullable: false),
                    f = table.Column<int>(type: "int", nullable: false),
                    fh = table.Column<int>(type: "int", nullable: false),
                    rx0 = table.Column<int>(type: "int", nullable: false),
                    rx1 = table.Column<int>(type: "int", nullable: false),
                    x = table.Column<int>(type: "int", nullable: false),
                    y = table.Column<int>(type: "int", nullable: false),
                    hide = table.Column<int>(type: "int", nullable: false),
                    mobtime = table.Column<int>(type: "int", nullable: false),
                    team = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("plife_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reporttime = table.Column<long>(type: "bigint", nullable: false),
                    reporterid = table.Column<int>(type: "int", nullable: false),
                    victimid = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<short>(type: "smallint", nullable: false),
                    chatlog = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reports_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    ringId1 = table.Column<long>(type: "bigint", nullable: false),
                    ringId2 = table.Column<long>(type: "bigint", nullable: false),
                    characterId1 = table.Column<int>(type: "int", nullable: false),
                    characterId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rings_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_drop_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dropperid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_drop_data_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_drop_data_global",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    continent = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "'-1'"),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    comments = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_drop_data_global_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NpcId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_gachapon_pool_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool_item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PoolId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_gachapon_pool_item_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool_level_chance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PoolId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Chance = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_gachapon_pool_level_chance_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sys_monstercarddata",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cardid = table.Column<int>(type: "int", nullable: false),
                    mobid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_monstercarddata_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_reactordrops",
                columns: table => new
                {
                    reactordropid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reactorid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    questid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_reactordrops_pkey", x => x.reactordropid);
                });

            migrationBuilder.CreateTable(
                name: "sys_shopitems",
                columns: table => new
                {
                    shopitemid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shopid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    pitch = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false, comment: "sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_shopitems_pkey", x => x.shopitemid);
                });

            migrationBuilder.CreateTable(
                name: "sys_shops",
                columns: table => new
                {
                    shopid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    npcid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_shops_pkey", x => x.shopid);
                });

            migrationBuilder.CreateTable(
                name: "sys_specialcashitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sn = table.Column<int>(type: "int", nullable: false),
                    modifier = table.Column<int>(type: "int", nullable: false, comment: "1024 is add/remove"),
                    info = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sys_specialcashitems_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "family_character",
                columns: table => new
                {
                    cid = table.Column<int>(type: "integer", nullable: false),
                    familyid = table.Column<int>(type: "int", nullable: false),
                    seniorid = table.Column<int>(type: "int", nullable: false),
                    reputation = table.Column<int>(type: "int", nullable: false),
                    todaysrep = table.Column<int>(type: "int", nullable: false),
                    totalreputation = table.Column<int>(type: "int", nullable: false),
                    reptosenior = table.Column<int>(type: "int", nullable: false),
                    precepts = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    lastresettime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("family_character_pkey", x => x.cid);
                    table.ForeignKey(
                        name: "family_character_ibfk_1",
                        column: x => x.cid,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "accountid",
                table: "account_ban",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "accountid1",
                table: "account_bindings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "id",
                table: "accounts",
                columns: new[] { "id", "name" });

            migrationBuilder.CreateIndex(
                name: "id_2",
                table: "accounts",
                columns: new[] { "id", "nxCredit", "maplePoint", "nxPrepaid" });

            migrationBuilder.CreateIndex(
                name: "name",
                table: "accounts",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "name1",
                table: "alliance",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "accountid2",
                table: "characters",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "id_21",
                table: "characters",
                columns: new[] { "id", "accountid", "name" });

            migrationBuilder.CreateIndex(
                name: "ranking1",
                table: "characters",
                columns: new[] { "level", "exp" });

            migrationBuilder.CreateIndex(
                name: "idx_cdk_codes_code",
                table: "ex_reward_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_cdk_items_code",
                table: "ex_reward_items",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "idx_code_records",
                table: "ex_reward_records",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "cid",
                table: "family_character",
                columns: new[] { "cid", "familyid" });

            migrationBuilder.CreateIndex(
                name: "charid",
                table: "family_entitlement",
                column: "charid");

            migrationBuilder.CreateIndex(
                name: "cid_2",
                table: "fredstorage",
                column: "cid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "guildid",
                table: "guilds",
                columns: new[] { "guildid", "name" });

            migrationBuilder.CreateIndex(
                name: "hwid_2",
                table: "hwidbans",
                column: "hwid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "mac_2",
                table: "macbans",
                column: "mac",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "characterid",
                table: "namechanges",
                column: "characterid");

            migrationBuilder.CreateIndex(
                name: "dropperid",
                table: "sys_drop_data",
                columns: new[] { "dropperid", "itemid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "dropperid_2",
                table: "sys_drop_data",
                columns: new[] { "dropperid", "itemid" });

            migrationBuilder.CreateIndex(
                name: "mobid",
                table: "sys_drop_data",
                column: "dropperid");

            migrationBuilder.CreateIndex(
                name: "mobid1",
                table: "sys_drop_data_global",
                column: "continent");

            migrationBuilder.CreateIndex(
                name: "id1",
                table: "sys_monstercarddata",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "reactorid",
                table: "sys_reactordrops",
                column: "reactorid");

            var sqlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData");
            void RunSeed(string script) =>
                migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, script)));

            // 种子脚本用显式 id 插入数据，PostgreSQL 的 identity 序列不会随之推进，
            // 需在插入后手动校准，否则后续不带 id 的 INSERT 会撞主键。
            void ResetSequences()
            {
                foreach (var (table, column) in new[]
                {
                ("account_ban", "Id"),
                ("account_bindings", "Id"),
                ("accounts", "id"),
                ("alliance", "id"),
                ("bbs_replies", "replyid"),
                ("bbs_threads", "threadid"),
                ("boss_log", "id"),
                ("characterexplogs", "id"),
                ("characters", "id"),
                ("dueypackages", "Id"),
                ("ex_reward_codes", "Id"),
                ("ex_reward_items", "Id"),
                ("ex_reward_records", "Id"),
                ("family_entitlement", "id"),
                ("fredstorage", "id"),
                ("gifts", "id"),
                ("guilds", "guildid"),
                ("hwidbans", "hwidbanid"),
                ("ipbans", "ipbanid"),
                ("macbans", "macbanid"),
                ("marriages", "marriageid"),
                ("mts_cart", "id"),
                ("mts_items", "id"),
                ("namechanges", "id"),
                ("newyear", "id"),
                ("notes", "id"),
                ("playernpcs", "id"),
                ("playernpcs_equip", "id"),
                ("playernpcs_field", "id"),
                ("plife", "id"),
                ("reports", "id"),
                ("rings", "id"),
                ("sys_drop_data", "id"),
                ("sys_drop_data_global", "id"),
                ("sys_gachapon_pool", "Id"),
                ("sys_gachapon_pool_item", "Id"),
                ("sys_gachapon_pool_level_chance", "Id"),
                ("sys_monstercarddata", "id"),
                ("sys_reactordrops", "reactordropid"),
                ("sys_shopitems", "shopitemid"),
                ("sys_shops", "shopid"),
                ("sys_specialcashitems", "id")
                })
                {
                    migrationBuilder.Sql(
                        $"SELECT setval(pg_get_serial_sequence('\"{table}\"', '{column}'), " +
                        $"COALESCE((SELECT MAX(\"{column}\") FROM \"{table}\"), 1));");
                }
            }

            RunSeed("v0.0.1-data_mob_drop.sql");
            RunSeed("v0.0.1-data_shop.sql");
            RunSeed("v0.0.1-data_other.sql");
            RunSeed("v0.0.1-gachapon.sql");

            // dongfangshenzhou 含不带 id 的 INSERT INTO sys_shopitems，
            // 必须先校准序列再执行，否则与已插入的数据撞主键。
            ResetSequences();
            RunSeed("v0.0.1-dongfangshenzhou.sql");
            ResetSequences();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_ban");

            migrationBuilder.DropTable(
                name: "account_bindings");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "alliance");

            migrationBuilder.DropTable(
                name: "bbs_replies");

            migrationBuilder.DropTable(
                name: "bbs_threads");

            migrationBuilder.DropTable(
                name: "boss_log");

            migrationBuilder.DropTable(
                name: "characterexplogs");

            migrationBuilder.DropTable(
                name: "dueypackages");

            migrationBuilder.DropTable(
                name: "ex_reward_codes");

            migrationBuilder.DropTable(
                name: "ex_reward_items");

            migrationBuilder.DropTable(
                name: "ex_reward_records");

            migrationBuilder.DropTable(
                name: "family_character");

            migrationBuilder.DropTable(
                name: "family_entitlement");

            migrationBuilder.DropTable(
                name: "fredstorage");

            migrationBuilder.DropTable(
                name: "gifts");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "hwidaccounts");

            migrationBuilder.DropTable(
                name: "hwidbans");

            migrationBuilder.DropTable(
                name: "ipbans");

            migrationBuilder.DropTable(
                name: "macbans");

            migrationBuilder.DropTable(
                name: "marriages");

            migrationBuilder.DropTable(
                name: "mts_cart");

            migrationBuilder.DropTable(
                name: "mts_items");

            migrationBuilder.DropTable(
                name: "namechanges");

            migrationBuilder.DropTable(
                name: "newyear");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "playernpcs");

            migrationBuilder.DropTable(
                name: "playernpcs_equip");

            migrationBuilder.DropTable(
                name: "playernpcs_field");

            migrationBuilder.DropTable(
                name: "plife");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "rings");

            migrationBuilder.DropTable(
                name: "sys_drop_data");

            migrationBuilder.DropTable(
                name: "sys_drop_data_global");

            migrationBuilder.DropTable(
                name: "sys_gachapon_pool");

            migrationBuilder.DropTable(
                name: "sys_gachapon_pool_item");

            migrationBuilder.DropTable(
                name: "sys_gachapon_pool_level_chance");

            migrationBuilder.DropTable(
                name: "sys_monstercarddata");

            migrationBuilder.DropTable(
                name: "sys_reactordrops");

            migrationBuilder.DropTable(
                name: "sys_shopitems");

            migrationBuilder.DropTable(
                name: "sys_shops");

            migrationBuilder.DropTable(
                name: "sys_specialcashitems");

            migrationBuilder.DropTable(
                name: "characters");
        }
    }
}

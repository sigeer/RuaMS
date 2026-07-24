using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BanLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<sbyte>(type: "tinyint", nullable: false),
                    ReasonDescription = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_bindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MAC = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HWID = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastActiveTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pin = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pic = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    createdat = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    birthday = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "'2005-05-11'"),
                    nxCredit = table.Column<int>(type: "int", nullable: false),
                    maplePoint = table.Column<int>(type: "int", nullable: false),
                    nxPrepaid = table.Column<int>(type: "int", nullable: false),
                    characterslots = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'3'"),
                    gender = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'10'"),
                    tos = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    gmlevel = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    nick = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Blob = table.Column<byte[]>(type: "MEDIUMBLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alliance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'2'"),
                    notice = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank1 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Master'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank2 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Jr. Master'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank3 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank4 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank5 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bbs_replies",
                columns: table => new
                {
                    replyid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    threadid = table.Column<int>(type: "int", nullable: false),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.replyid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bbs_threads",
                columns: table => new
                {
                    threadid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    icon = table.Column<short>(type: "smallint", nullable: false),
                    replycount = table.Column<short>(type: "smallint", nullable: false),
                    startpost = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    guildid = table.Column<int>(type: "int", nullable: false),
                    localthreadid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.threadid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bosslog_daily",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempttime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bosslog_weekly",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempttime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cdk_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expiration = table.Column<long>(type: "bigint", nullable: false),
                    MaxCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cdk_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'4000000'"),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cdk_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    RecipientTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "characterexplogs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int", nullable: false),
                    world_exp_rate = table.Column<int>(type: "int", nullable: false),
                    exp_coupon = table.Column<int>(type: "int", nullable: false),
                    gained_exp = table.Column<long>(type: "bigint", nullable: false),
                    current_exp = table.Column<int>(type: "int", nullable: false),
                    exp_gain_time = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    sp = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValueSql: "'0,0,0,0,0,0,0,0,0,0'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    map = table.Column<int>(type: "int", nullable: false),
                    spawnpoint = table.Column<int>(type: "int", nullable: false),
                    buddyCapacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'25'"),
                    createdate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    finishedDojoTutorial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    vanquisherKills = table.Column<int>(type: "int", nullable: false),
                    summonValue = table.Column<int>(type: "int", nullable: false),
                    PQPoints = table.Column<int>(type: "int", nullable: false),
                    dataString = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastLogoutTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    lastExpGainTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    partySearch = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    jailexpire = table.Column<long>(type: "bigint", nullable: false),
                    HpAlert = table.Column<int>(type: "int", nullable: false),
                    MpAlert = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Blob = table.Column<byte[]>(type: "MEDIUMBLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dueypackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Mesos = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    CreateTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    Message = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasNotified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Type = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ItemBlob = table.Column<byte[]>(type: "MEDIUMBLOB", nullable: true),
                    ClaimTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PackageId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_entitlement",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int", nullable: false),
                    entitlementid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fredstorage",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cid = table.Column<int>(type: "int", nullable: false),
                    daynotes = table.Column<int>(type: "int", nullable: false),
                    meso = table.Column<int>(type: "int", nullable: false),
                    itemMeso = table.Column<long>(type: "bigint", nullable: false),
                    ItemsBlob = table.Column<byte[]>(type: "MEDIUMBLOB", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "tinytext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sn = table.Column<int>(type: "int", nullable: false),
                    ringSourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    guildid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    leader = table.Column<int>(type: "int", nullable: false),
                    GP = table.Column<int>(type: "int", nullable: false),
                    logo = table.Column<int>(type: "int", nullable: false),
                    logoColor = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank1title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Master'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank2title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Jr. Master'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank3title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank4title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rank5title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'10'"),
                    logoBG = table.Column<int>(type: "int", nullable: false),
                    logoBGColor = table.Column<short>(type: "smallint", nullable: false),
                    notice = table.Column<string>(type: "varchar(101)", maxLength: 101, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    signature = table.Column<int>(type: "int", nullable: false),
                    allianceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.guildid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hwidaccounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "int", nullable: false),
                    hwid = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    relevance = table.Column<sbyte>(type: "tinyint", nullable: false),
                    expiresat = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.accountid, x.hwid });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hwidbans",
                columns: table => new
                {
                    hwidbanid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    hwid = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.hwidbanid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ipbans",
                columns: table => new
                {
                    ipbanid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ip = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    aid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ipbanid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "macbans",
                columns: table => new
                {
                    macbanid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mac = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    aid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macbanid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marriages",
                columns: table => new
                {
                    marriageid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    husbandid = table.Column<int>(type: "int", nullable: false),
                    wifeid = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Time0 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Time1 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Time2 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    RingSourceId = table.Column<int>(type: "int", nullable: false),
                    EngagementItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.marriageid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mts_cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mts_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    owner = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sellername = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sell_ends = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transfer = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    vicious = table.Column<int>(type: "int", nullable: false),
                    flag = table.Column<int>(type: "int", nullable: false),
                    expiration = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "namechanges",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    old = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    @new = table.Column<string>(name: "new", type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requestTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completionTime = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "newyear",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    senderid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    receiverid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    message = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    senderdiscard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    receiverdiscard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    received = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    timesent = table.Column<long>(type: "bigint", nullable: false),
                    timereceived = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    fame = table.Column<int>(type: "int", nullable: false),
                    deleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs_equip",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    npcid = table.Column<int>(type: "int", nullable: false),
                    equipid = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    equippos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs_field",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    world = table.Column<int>(type: "int", nullable: false),
                    map = table.Column<int>(type: "int", nullable: false),
                    step = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    podium = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plife",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    world = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    map = table.Column<int>(type: "int", nullable: false),
                    life = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false, defaultValueSql: "'n'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reporttime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    reporterid = table.Column<int>(type: "int", nullable: false),
                    victimid = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<sbyte>(type: "tinyint", nullable: false),
                    chatlog = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    ringId1 = table.Column<long>(type: "bigint", nullable: false),
                    ringId2 = table.Column<long>(type: "bigint", nullable: false),
                    characterId1 = table.Column<int>(type: "int", nullable: false),
                    characterId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_drop_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dropperid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_drop_data_global",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    continent = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'-1'"),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    comments = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NpcId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool_item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_gachapon_pool_level_chance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_monstercarddata",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cardid = table.Column<int>(type: "int", nullable: false),
                    mobid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_reactordrops",
                columns: table => new
                {
                    reactordropid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reactorid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    questid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.reactordropid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_shopitems",
                columns: table => new
                {
                    shopitemid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shopid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    pitch = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false, comment: "sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopitemid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_shops",
                columns: table => new
                {
                    shopid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    npcid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_specialcashitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sn = table.Column<int>(type: "int", nullable: false),
                    modifier = table.Column<int>(type: "int", nullable: false, comment: "1024 is add/remove"),
                    info = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_character",
                columns: table => new
                {
                    cid = table.Column<int>(type: "int", nullable: false),
                    familyid = table.Column<int>(type: "int", nullable: false),
                    seniorid = table.Column<int>(type: "int", nullable: false),
                    reputation = table.Column<int>(type: "int", nullable: false),
                    todaysrep = table.Column<int>(type: "int", nullable: false),
                    totalreputation = table.Column<int>(type: "int", nullable: false),
                    reptosenior = table.Column<int>(type: "int", nullable: false),
                    precepts = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastresettime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cid);
                    table.ForeignKey(
                        name: "family_character_ibfk_1",
                        column: x => x.cid,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "idx_code",
                table: "cdk_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_code2",
                table: "cdk_items",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "idx_code1",
                table: "cdk_records",
                column: "CodeId");

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
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_mob_drop.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_shop.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_other.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-gachapon.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-dongfangshenzhou.sql")));
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
                name: "bosslog_daily");

            migrationBuilder.DropTable(
                name: "bosslog_weekly");

            migrationBuilder.DropTable(
                name: "cdk_codes");

            migrationBuilder.DropTable(
                name: "cdk_items");

            migrationBuilder.DropTable(
                name: "cdk_records");

            migrationBuilder.DropTable(
                name: "characterexplogs");

            migrationBuilder.DropTable(
                name: "dueypackages");

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

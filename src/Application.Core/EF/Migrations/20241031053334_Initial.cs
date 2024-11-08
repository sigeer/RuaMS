using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    password = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValueSql: "''"),
                    pin = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValueSql: "''"),
                    pic = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    loggedin = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    lastlogin = table.Column<DateTimeOffset>(type: "timestamp", nullable: true),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    birthday = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "'2005-05-11'"),
                    banned = table.Column<sbyte>(type: "tinyint", nullable: false),
                    banreason = table.Column<string>(type: "text", nullable: true),
                    macs = table.Column<string>(type: "tinytext", nullable: true),
                    nxCredit = table.Column<int>(type: "int(11)", nullable: true),
                    maplePoint = table.Column<int>(type: "int(11)", nullable: true),
                    nxPrepaid = table.Column<int>(type: "int(11)", nullable: true),
                    characterslots = table.Column<sbyte>(type: "tinyint(2)", nullable: false, defaultValueSql: "'3'"),
                    gender = table.Column<sbyte>(type: "tinyint(2)", nullable: false, defaultValueSql: "'10'"),
                    tempban = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2005-05-11 00:00:00'"),
                    greason = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    tos = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sitelogged = table.Column<string>(type: "text", nullable: true),
                    webadmin = table.Column<int>(type: "int(1)", nullable: true, defaultValueSql: "'0'"),
                    nick = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    mute = table.Column<int>(type: "int(1)", nullable: true, defaultValueSql: "'0'"),
                    email = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    ip = table.Column<string>(type: "text", nullable: true),
                    rewardpoints = table.Column<int>(type: "int(11)", nullable: false),
                    votepoints = table.Column<int>(type: "int(11)", nullable: false),
                    hwid = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false, defaultValueSql: "''"),
                    language = table.Column<int>(type: "int(1)", nullable: false, defaultValueSql: "'2'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alliance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    capacity = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'2'"),
                    notice = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "''"),
                    rank1 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Master'"),
                    rank2 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank4 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank5 = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "allianceguilds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    allianceid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'"),
                    guildid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "area_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int(11)", nullable: false),
                    area = table.Column<int>(type: "int(11)", nullable: false),
                    info = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bbs_replies",
                columns: table => new
                {
                    replyid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    threadid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    postercid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    timestamp = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    content = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.replyid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bbs_threads",
                columns: table => new
                {
                    threadid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    postercid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    timestamp = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    icon = table.Column<short>(type: "smallint(5) unsigned", nullable: false),
                    replycount = table.Column<short>(type: "smallint(5) unsigned", nullable: false),
                    startpost = table.Column<string>(type: "text", nullable: false),
                    guildid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    localthreadid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.threadid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bosslog_daily",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false),
                    attempttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bosslog_weekly",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false),
                    attempttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "buddies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    buddyid = table.Column<int>(type: "int(11)", nullable: false),
                    pending = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    group = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "characterexplogs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int", nullable: false),
                    world_exp_rate = table.Column<int>(type: "int", nullable: false),
                    exp_coupon = table.Column<int>(type: "int", nullable: false),
                    gained_exp = table.Column<long>(type: "bigint", nullable: false),
                    current_exp = table.Column<int>(type: "int", nullable: false),
                    exp_gain_time = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    accountid = table.Column<int>(type: "int(11)", nullable: false),
                    world = table.Column<int>(type: "int(11)", nullable: false),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    level = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    exp = table.Column<int>(type: "int(11)", nullable: false),
                    gachaexp = table.Column<int>(type: "int(11)", nullable: false),
                    str = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'12'"),
                    dex = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'5'"),
                    luk = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'4'"),
                    @int = table.Column<int>(name: "int", type: "int(11)", nullable: false, defaultValueSql: "'4'"),
                    hp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'50'"),
                    mp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'5'"),
                    maxhp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'50'"),
                    maxmp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'5'"),
                    meso = table.Column<int>(type: "int(11)", nullable: false),
                    hpMpUsed = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    job = table.Column<int>(type: "int(11)", nullable: false),
                    skincolor = table.Column<int>(type: "int(11)", nullable: false),
                    gender = table.Column<int>(type: "int(11)", nullable: false),
                    fame = table.Column<int>(type: "int(11)", nullable: false),
                    fquest = table.Column<int>(type: "int(11)", nullable: false),
                    hair = table.Column<int>(type: "int(11)", nullable: false),
                    face = table.Column<int>(type: "int(11)", nullable: false),
                    ap = table.Column<int>(type: "int(11)", nullable: false),
                    sp = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValueSql: "'0,0,0,0,0,0,0,0,0,0'"),
                    map = table.Column<int>(type: "int(11)", nullable: false),
                    spawnpoint = table.Column<int>(type: "int(11)", nullable: false),
                    gm = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    party = table.Column<int>(type: "int(11)", nullable: false),
                    buddyCapacity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'25'"),
                    createdate = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    rank = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'1'"),
                    rankMove = table.Column<int>(type: "int(11)", nullable: false),
                    jobRank = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'1'"),
                    jobRankMove = table.Column<int>(type: "int(11)", nullable: false),
                    guildid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    guildrank = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'5'"),
                    messengerid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    messengerposition = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'4'"),
                    mountlevel = table.Column<int>(type: "int(9)", nullable: false, defaultValueSql: "'1'"),
                    mountexp = table.Column<int>(type: "int(9)", nullable: false),
                    mounttiredness = table.Column<int>(type: "int(9)", nullable: false),
                    omokwins = table.Column<int>(type: "int(11)", nullable: false),
                    omoklosses = table.Column<int>(type: "int(11)", nullable: false),
                    omokties = table.Column<int>(type: "int(11)", nullable: false),
                    matchcardwins = table.Column<int>(type: "int(11)", nullable: false),
                    matchcardlosses = table.Column<int>(type: "int(11)", nullable: false),
                    matchcardties = table.Column<int>(type: "int(11)", nullable: false),
                    MerchantMesos = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    HasMerchant = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    equipslots = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'24'"),
                    useslots = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'24'"),
                    setupslots = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'24'"),
                    etcslots = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'24'"),
                    familyId = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'-1'"),
                    monsterbookcover = table.Column<int>(type: "int(11)", nullable: false),
                    allianceRank = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'5'"),
                    vanquisherStage = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    ariantPoints = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    dojoPoints = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    lastDojoStage = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    finishedDojoTutorial = table.Column<sbyte>(type: "tinyint(1) unsigned", nullable: false),
                    vanquisherKills = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    summonValue = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    partnerId = table.Column<int>(type: "int(11)", nullable: false),
                    marriageItemId = table.Column<int>(type: "int(11)", nullable: false),
                    reborns = table.Column<int>(type: "int(5)", nullable: false),
                    PQPoints = table.Column<int>(type: "int(11)", nullable: false),
                    dataString = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, defaultValueSql: "''"),
                    lastLogoutTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    lastExpGainTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    partySearch = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    jailexpire = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cooldowns",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int(11)", nullable: false),
                    SkillID = table.Column<int>(type: "int(11)", nullable: false),
                    length = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    StartTime = table.Column<long>(type: "bigint(20) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "drop_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    dropperid = table.Column<int>(type: "int(11)", nullable: false),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int(11)", nullable: false),
                    chance = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "drop_data_global",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    continent = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'-1'"),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int(11)", nullable: false),
                    chance = table.Column<int>(type: "int(11)", nullable: false),
                    comments = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dueypackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ReceiverId = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    SenderName = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    Mesos = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'0'"),
                    TimeStamp = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    Message = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Checked = table.Column<sbyte>(type: "tinyint(1) unsigned", nullable: false, defaultValue: (sbyte)1),
                    Type = table.Column<sbyte>(type: "tinyint(1) unsigned", nullable: false, defaultValue: (sbyte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PackageId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "eventstats",
                columns: table => new
                {
                    characterid = table.Column<int>(type: "int(11) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false, defaultValueSql: "'0'", comment: "0"),
                    info = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.characterid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_entitlement",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int(11)", nullable: false),
                    entitlementid = table.Column<int>(type: "int(11)", nullable: false),
                    timestamp = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fredstorage",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    daynotes = table.Column<int>(type: "int(4) unsigned", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    to = table.Column<int>(type: "int(11)", nullable: false),
                    from = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    message = table.Column<string>(type: "tinytext", nullable: false),
                    sn = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    ringid = table.Column<int>(type: "int(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    guildid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    leader = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    GP = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    logo = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    logoColor = table.Column<short>(type: "smallint(5) unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false),
                    rank1title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Master'"),
                    rank2title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank4title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank5title = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    capacity = table.Column<int>(type: "int(10) unsigned", nullable: false, defaultValueSql: "'10'"),
                    logoBG = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    logoBGColor = table.Column<short>(type: "smallint(5) unsigned", nullable: false),
                    notice = table.Column<string>(type: "varchar(101)", maxLength: 101, nullable: true),
                    signature = table.Column<int>(type: "int(11)", nullable: false),
                    allianceId = table.Column<int>(type: "int(11) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.guildid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hwidaccounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "int(11)", nullable: false),
                    hwid = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    relevance = table.Column<sbyte>(type: "tinyint(2)", nullable: false),
                    expiresat = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.accountid, x.hwid });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hwidbans",
                columns: table => new
                {
                    hwidbanid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    hwid = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.hwidbanid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventoryequipment",
                columns: table => new
                {
                    inventoryequipmentid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    inventoryitemid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    upgradeslots = table.Column<int>(type: "int(11)", nullable: false),
                    level = table.Column<int>(type: "int(11)", nullable: false),
                    str = table.Column<int>(type: "int(11)", nullable: false),
                    dex = table.Column<int>(type: "int(11)", nullable: false),
                    @int = table.Column<int>(name: "int", type: "int(11)", nullable: false),
                    luk = table.Column<int>(type: "int(11)", nullable: false),
                    hp = table.Column<int>(type: "int(11)", nullable: false),
                    mp = table.Column<int>(type: "int(11)", nullable: false),
                    watk = table.Column<int>(type: "int(11)", nullable: false),
                    matk = table.Column<int>(type: "int(11)", nullable: false),
                    wdef = table.Column<int>(type: "int(11)", nullable: false),
                    mdef = table.Column<int>(type: "int(11)", nullable: false),
                    acc = table.Column<int>(type: "int(11)", nullable: false),
                    avoid = table.Column<int>(type: "int(11)", nullable: false),
                    hands = table.Column<int>(type: "int(11)", nullable: false),
                    speed = table.Column<int>(type: "int(11)", nullable: false),
                    jump = table.Column<int>(type: "int(11)", nullable: false),
                    locked = table.Column<int>(type: "int(11)", nullable: false),
                    vicious = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    itemlevel = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    itemexp = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    ringid = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventoryequipmentid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventoryitems",
                columns: table => new
                {
                    inventoryitemid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    type = table.Column<sbyte>(type: "tinyint(3) unsigned", nullable: false),
                    characterid = table.Column<int>(type: "int(11)", nullable: true),
                    accountid = table.Column<int>(type: "int(11)", nullable: true),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    inventorytype = table.Column<int>(type: "int(11)", nullable: false),
                    position = table.Column<int>(type: "int(11)", nullable: false),
                    quantity = table.Column<int>(type: "int(11)", nullable: false),
                    owner = table.Column<string>(type: "tinytext", nullable: false),
                    petid = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'-1'"),
                    flag = table.Column<int>(type: "int(11)", nullable: false),
                    expiration = table.Column<long>(type: "bigint(20)", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventoryitemid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventorymerchant",
                columns: table => new
                {
                    inventorymerchantid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    inventoryitemid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    characterid = table.Column<int>(type: "int(11)", nullable: true),
                    bundles = table.Column<int>(type: "int(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventorymerchantid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ipbans",
                columns: table => new
                {
                    ipbanid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ip = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    aid = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ipbanid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "keymap",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    key = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false),
                    action = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "macbans",
                columns: table => new
                {
                    macbanid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    mac = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    aid = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macbanid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "macfilters",
                columns: table => new
                {
                    macfilterid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    filter = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macfilterid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "makercreatedata",
                columns: table => new
                {
                    id = table.Column<sbyte>(type: "tinyint(3) unsigned", nullable: false),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    req_level = table.Column<sbyte>(type: "tinyint(3) unsigned", nullable: false),
                    req_maker_level = table.Column<sbyte>(type: "tinyint(3) unsigned", nullable: false),
                    req_meso = table.Column<int>(type: "int(11)", nullable: false),
                    req_item = table.Column<int>(type: "int(11)", nullable: false),
                    req_equip = table.Column<int>(type: "int(11)", nullable: false),
                    catalyst = table.Column<int>(type: "int(11)", nullable: false),
                    quantity = table.Column<short>(type: "smallint(6)", nullable: false),
                    tuc = table.Column<sbyte>(type: "tinyint(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.itemid });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "makerreagentdata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    stat = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    value = table.Column<short>(type: "smallint(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.itemid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "makerrecipedata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    req_item = table.Column<int>(type: "int(11)", nullable: false),
                    count = table.Column<short>(type: "smallint(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.itemid, x.req_item });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "makerrewarddata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    rewardid = table.Column<int>(type: "int(11)", nullable: false),
                    quantity = table.Column<short>(type: "smallint(6)", nullable: false),
                    prob = table.Column<sbyte>(type: "tinyint(3) unsigned", nullable: false, defaultValueSql: "'100'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.itemid, x.rewardid });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marriages",
                columns: table => new
                {
                    marriageid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    husbandid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    wifeid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.marriageid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medalmaps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    queststatusid = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    mapid = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "monsterbook",
                columns: table => new
                {
                    charid = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    cardid = table.Column<int>(type: "int(11)", nullable: false),
                    level = table.Column<int>(type: "int(1)", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monsterbook", x => new { x.cardid, x.charid });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "monstercarddata",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cardid = table.Column<int>(type: "int(11)", nullable: false),
                    mobid = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mts_cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cid = table.Column<int>(type: "int(11)", nullable: false),
                    itemid = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mts_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tab = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false),
                    itemid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    seller = table.Column<int>(type: "int(11)", nullable: false),
                    price = table.Column<int>(type: "int(11)", nullable: false),
                    bid_incre = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    buy_now = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    position = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    upgradeslots = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    level = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    itemlevel = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    itemexp = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    ringid = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'-1'"),
                    str = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    dex = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    @int = table.Column<int>(name: "int", type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    luk = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    hp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    mp = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    watk = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    matk = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    wdef = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    mdef = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    acc = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    avoid = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    hands = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    speed = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    jump = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    locked = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'0'"),
                    isequip = table.Column<int>(type: "int(1)", nullable: false, defaultValueSql: "'0'"),
                    owner = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, defaultValueSql: "''"),
                    sellername = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    sell_ends = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    transfer = table.Column<int>(type: "int(2)", nullable: false, defaultValueSql: "'0'"),
                    vicious = table.Column<int>(type: "int(2) unsigned", nullable: false),
                    flag = table.Column<int>(type: "int(2) unsigned", nullable: false),
                    expiration = table.Column<long>(type: "bigint(20)", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "namechanges",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    old = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    @new = table.Column<string>(name: "new", type: "varchar(13)", maxLength: 13, nullable: false),
                    requestTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completionTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "newyear",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    senderid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'"),
                    sendername = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    receiverid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'"),
                    receivername = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    message = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false, defaultValueSql: "''"),
                    senderdiscard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    receiverdiscard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    received = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    timesent = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    timereceived = table.Column<long>(type: "bigint(20) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    to = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    from = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    message = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    fame = table.Column<int>(type: "int(11)", nullable: false),
                    deleted = table.Column<int>(type: "int(2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nxcode",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false),
                    retriever = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: true),
                    expiration = table.Column<long>(type: "bigint(20) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nxcode_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    codeid = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'5'"),
                    item = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'4000000'"),
                    quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nxcoupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    couponid = table.Column<int>(type: "int(11)", nullable: false),
                    rate = table.Column<int>(type: "int(11)", nullable: false),
                    activeday = table.Column<int>(type: "int(11)", nullable: false),
                    starthour = table.Column<int>(type: "int(11)", nullable: false),
                    endhour = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pets",
                columns: table => new
                {
                    petid = table.Column<int>(type: "int(11) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: true),
                    level = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    closeness = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    fullness = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    summoned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    flag = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.petid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playerdiseases",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int(11)", nullable: false),
                    disease = table.Column<int>(type: "int(11)", nullable: false),
                    mobskillid = table.Column<int>(type: "int(11)", nullable: false),
                    mobskilllv = table.Column<int>(type: "int(11)", nullable: false),
                    length = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    hair = table.Column<int>(type: "int(11)", nullable: false),
                    face = table.Column<int>(type: "int(11)", nullable: false),
                    skin = table.Column<int>(type: "int(11)", nullable: false),
                    gender = table.Column<int>(type: "int(11)", nullable: false),
                    x = table.Column<int>(type: "int(11)", nullable: false),
                    cy = table.Column<int>(type: "int(11)", nullable: false),
                    world = table.Column<int>(type: "int(11)", nullable: false),
                    map = table.Column<int>(type: "int(11)", nullable: false),
                    dir = table.Column<int>(type: "int(11)", nullable: false),
                    scriptid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    fh = table.Column<int>(type: "int(11)", nullable: false),
                    rx0 = table.Column<int>(type: "int(11)", nullable: false),
                    rx1 = table.Column<int>(type: "int(11)", nullable: false),
                    worldrank = table.Column<int>(type: "int(11)", nullable: false),
                    overallrank = table.Column<int>(type: "int(11)", nullable: false),
                    worldjobrank = table.Column<int>(type: "int(11)", nullable: false),
                    job = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs_equip",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    npcid = table.Column<int>(type: "int(11)", nullable: false),
                    equipid = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false),
                    equippos = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "playernpcs_field",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    world = table.Column<int>(type: "int(11)", nullable: false),
                    map = table.Column<int>(type: "int(11)", nullable: false),
                    step = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    podium = table.Column<short>(type: "smallint(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plife",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    world = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'-1'"),
                    map = table.Column<int>(type: "int(11)", nullable: false),
                    life = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false, defaultValueSql: "'n'"),
                    cy = table.Column<int>(type: "int(11)", nullable: false),
                    f = table.Column<int>(type: "int(11)", nullable: false),
                    fh = table.Column<int>(type: "int(11)", nullable: false),
                    rx0 = table.Column<int>(type: "int(11)", nullable: false),
                    rx1 = table.Column<int>(type: "int(11)", nullable: false),
                    x = table.Column<int>(type: "int(11)", nullable: false),
                    y = table.Column<int>(type: "int(11)", nullable: false),
                    hide = table.Column<int>(type: "int(11)", nullable: false),
                    mobtime = table.Column<int>(type: "int(11)", nullable: false),
                    team = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questactions",
                columns: table => new
                {
                    questactionid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    questid = table.Column<int>(type: "int(11)", nullable: false),
                    status = table.Column<int>(type: "int(11)", nullable: false),
                    data = table.Column<byte[]>(type: "blob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.questactionid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questprogress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    queststatusid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    progressid = table.Column<int>(type: "int(11)", nullable: false),
                    progress = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questrequirements",
                columns: table => new
                {
                    questrequirementid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    questid = table.Column<int>(type: "int(11)", nullable: false),
                    status = table.Column<int>(type: "int(11)", nullable: false),
                    data = table.Column<byte[]>(type: "blob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.questrequirementid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "queststatus",
                columns: table => new
                {
                    queststatusid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    quest = table.Column<int>(type: "int(11)", nullable: false),
                    status = table.Column<int>(type: "int(11)", nullable: false),
                    time = table.Column<int>(type: "int(11)", nullable: false),
                    expires = table.Column<long>(type: "bigint(20)", nullable: false),
                    forfeited = table.Column<int>(type: "int(11)", nullable: false),
                    completed = table.Column<int>(type: "int(11)", nullable: false),
                    info = table.Column<sbyte>(type: "tinyint(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.queststatusid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reactordrops",
                columns: table => new
                {
                    reactordropid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    reactorid = table.Column<int>(type: "int(11)", nullable: false),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    chance = table.Column<int>(type: "int(11)", nullable: false),
                    questid = table.Column<int>(type: "int(5)", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.reactordropid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    reporttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    reporterid = table.Column<int>(type: "int(11)", nullable: false),
                    victimid = table.Column<int>(type: "int(11)", nullable: false),
                    reason = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    chatlog = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "responses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    chat = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    partnerRingId = table.Column<int>(type: "int(11)", nullable: false),
                    partnerChrId = table.Column<int>(type: "int(11)", nullable: false),
                    partnername = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "savedlocations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    locationtype = table.Column<string>(type: "enum('FREE_MARKET','WORLDTOUR','FLORINA','INTRO','SUNDAY_MARKET','MIRROR','EVENT','BOSSPQ','HAPPYVILLE','DEVELOPER','MONSTER_CARNIVAL')", nullable: false),
                    map = table.Column<int>(type: "int(11)", nullable: false),
                    portal = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "server_queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    accountid = table.Column<int>(type: "int(11)", nullable: false),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<sbyte>(type: "tinyint(2)", nullable: false),
                    value = table.Column<int>(type: "int(10)", nullable: false),
                    message = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    createTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shopitems",
                columns: table => new
                {
                    shopitemid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    shopid = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    itemid = table.Column<int>(type: "int(11)", nullable: false),
                    price = table.Column<int>(type: "int(11)", nullable: false),
                    pitch = table.Column<int>(type: "int(11)", nullable: false),
                    position = table.Column<int>(type: "int(11)", nullable: false, comment: "sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopitemid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    shopid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    npcid = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "skillmacros",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    position = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    skill1 = table.Column<int>(type: "int(11)", nullable: false),
                    skill2 = table.Column<int>(type: "int(11)", nullable: false),
                    skill3 = table.Column<int>(type: "int(11)", nullable: false),
                    name = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: true),
                    shout = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "specialcashitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    sn = table.Column<int>(type: "int(11)", nullable: false),
                    modifier = table.Column<int>(type: "int(11)", nullable: false, comment: "1024 is add/remove"),
                    info = table.Column<int>(type: "int(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "storages",
                columns: table => new
                {
                    storageid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    accountid = table.Column<int>(type: "int(11)", nullable: false),
                    world = table.Column<int>(type: "int(2)", nullable: false),
                    slots = table.Column<int>(type: "int(11)", nullable: false),
                    meso = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.storageid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trocklocations",
                columns: table => new
                {
                    trockid = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    mapid = table.Column<int>(type: "int(11)", nullable: false),
                    vip = table.Column<int>(type: "int(2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.trockid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    charid = table.Column<int>(type: "int(11)", nullable: false),
                    sn = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "worldtransfers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    from = table.Column<sbyte>(type: "tinyint(3)", nullable: false),
                    to = table.Column<sbyte>(type: "tinyint(3)", nullable: false),
                    requestTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completionTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quickslotkeymapped",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "int(11)", nullable: false),
                    keymap = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.accountid);
                    table.ForeignKey(
                        name: "quickslotkeymapped_accountid_fk",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "famelog",
                columns: table => new
                {
                    famelogid = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    characterid_to = table.Column<int>(type: "int(11)", nullable: false),
                    when = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.famelogid);
                    table.ForeignKey(
                        name: "famelog_ibfk_1",
                        column: x => x.characterid,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_character",
                columns: table => new
                {
                    cid = table.Column<int>(type: "int(11)", nullable: false),
                    familyid = table.Column<int>(type: "int(11)", nullable: false),
                    seniorid = table.Column<int>(type: "int(11)", nullable: false),
                    reputation = table.Column<int>(type: "int(11)", nullable: false),
                    todaysrep = table.Column<int>(type: "int(11)", nullable: false),
                    totalreputation = table.Column<int>(type: "int(11)", nullable: false),
                    reptosenior = table.Column<int>(type: "int(11)", nullable: false),
                    precepts = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lastresettime = table.Column<long>(type: "bigint(20)", nullable: false)
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    skillid = table.Column<int>(type: "int(11)", nullable: false),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    skilllevel = table.Column<int>(type: "int(11)", nullable: false),
                    masterlevel = table.Column<int>(type: "int(11)", nullable: false),
                    expiration = table.Column<long>(type: "bigint(20)", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_skills_characters_characterid",
                        column: x => x.characterid,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dueyitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PackageId = table.Column<int>(type: "int(10) unsigned", nullable: false),
                    inventoryitemid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "dueyitems_ibfk_1",
                        column: x => x.PackageId,
                        principalTable: "dueypackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "petignores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    petid = table.Column<int>(type: "int(11) unsigned", nullable: false),
                    itemid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_petignorepetid",
                        column: x => x.petid,
                        principalTable: "pets",
                        principalColumn: "petid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
                name: "ranking1",
                table: "accounts",
                columns: new[] { "id", "banned" });

            migrationBuilder.CreateIndex(
                name: "name1",
                table: "alliance",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "accountid",
                table: "characters",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "id_21",
                table: "characters",
                columns: new[] { "id", "accountid", "name" });

            migrationBuilder.CreateIndex(
                name: "id1",
                table: "characters",
                columns: new[] { "id", "accountid", "world" });

            migrationBuilder.CreateIndex(
                name: "party",
                table: "characters",
                column: "party");

            migrationBuilder.CreateIndex(
                name: "ranking11",
                table: "characters",
                columns: new[] { "level", "exp" });

            migrationBuilder.CreateIndex(
                name: "ranking2",
                table: "characters",
                columns: new[] { "gm", "job" });

            migrationBuilder.CreateIndex(
                name: "dropperid",
                table: "drop_data",
                columns: new[] { "dropperid", "itemid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "dropperid_2",
                table: "drop_data",
                columns: new[] { "dropperid", "itemid" });

            migrationBuilder.CreateIndex(
                name: "mobid1",
                table: "drop_data",
                column: "dropperid");

            migrationBuilder.CreateIndex(
                name: "mobid",
                table: "drop_data_global",
                column: "continent");

            migrationBuilder.CreateIndex(
                name: "INVENTORYITEMID",
                table: "dueyitems",
                column: "inventoryitemid");

            migrationBuilder.CreateIndex(
                name: "PackageId",
                table: "dueyitems",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "characterid",
                table: "famelog",
                column: "characterid");

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
                name: "INVENTORYITEMID2",
                table: "inventoryequipment",
                column: "inventoryitemid");

            migrationBuilder.CreateIndex(
                name: "CHARID",
                table: "inventoryitems",
                column: "characterid");

            migrationBuilder.CreateIndex(
                name: "INVENTORYITEMID1",
                table: "inventorymerchant",
                column: "inventoryitemid");

            migrationBuilder.CreateIndex(
                name: "mac_2",
                table: "macbans",
                column: "mac",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "queststatusid",
                table: "medalmaps",
                column: "queststatusid");

            migrationBuilder.CreateIndex(
                name: "id2",
                table: "monstercarddata",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "characterid1",
                table: "namechanges",
                column: "characterid");

            migrationBuilder.CreateIndex(
                name: "code",
                table: "nxcode",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_petignorepetid",
                table: "petignores",
                column: "petid");

            migrationBuilder.CreateIndex(
                name: "reactorid",
                table: "reactordrops",
                column: "reactorid");

            migrationBuilder.CreateIndex(
                name: "skillpair",
                table: "skills",
                columns: new[] { "skillid", "characterid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "skills_chrid_fk",
                table: "skills",
                column: "characterid");

            migrationBuilder.CreateIndex(
                name: "characterid2",
                table: "worldtransfers",
                column: "characterid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alliance");

            migrationBuilder.DropTable(
                name: "allianceguilds");

            migrationBuilder.DropTable(
                name: "area_info");

            migrationBuilder.DropTable(
                name: "bbs_replies");

            migrationBuilder.DropTable(
                name: "bbs_threads");

            migrationBuilder.DropTable(
                name: "bosslog_daily");

            migrationBuilder.DropTable(
                name: "bosslog_weekly");

            migrationBuilder.DropTable(
                name: "buddies");

            migrationBuilder.DropTable(
                name: "characterexplogs");

            migrationBuilder.DropTable(
                name: "cooldowns");

            migrationBuilder.DropTable(
                name: "drop_data");

            migrationBuilder.DropTable(
                name: "drop_data_global");

            migrationBuilder.DropTable(
                name: "dueyitems");

            migrationBuilder.DropTable(
                name: "eventstats");

            migrationBuilder.DropTable(
                name: "famelog");

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
                name: "inventoryequipment");

            migrationBuilder.DropTable(
                name: "inventoryitems");

            migrationBuilder.DropTable(
                name: "inventorymerchant");

            migrationBuilder.DropTable(
                name: "ipbans");

            migrationBuilder.DropTable(
                name: "keymap");

            migrationBuilder.DropTable(
                name: "macbans");

            migrationBuilder.DropTable(
                name: "macfilters");

            migrationBuilder.DropTable(
                name: "makercreatedata");

            migrationBuilder.DropTable(
                name: "makerreagentdata");

            migrationBuilder.DropTable(
                name: "makerrecipedata");

            migrationBuilder.DropTable(
                name: "makerrewarddata");

            migrationBuilder.DropTable(
                name: "marriages");

            migrationBuilder.DropTable(
                name: "medalmaps");

            migrationBuilder.DropTable(
                name: "monsterbook");

            migrationBuilder.DropTable(
                name: "monstercarddata");

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
                name: "nxcode");

            migrationBuilder.DropTable(
                name: "nxcode_items");

            migrationBuilder.DropTable(
                name: "nxcoupons");

            migrationBuilder.DropTable(
                name: "petignores");

            migrationBuilder.DropTable(
                name: "playerdiseases");

            migrationBuilder.DropTable(
                name: "playernpcs");

            migrationBuilder.DropTable(
                name: "playernpcs_equip");

            migrationBuilder.DropTable(
                name: "playernpcs_field");

            migrationBuilder.DropTable(
                name: "plife");

            migrationBuilder.DropTable(
                name: "questactions");

            migrationBuilder.DropTable(
                name: "questprogress");

            migrationBuilder.DropTable(
                name: "questrequirements");

            migrationBuilder.DropTable(
                name: "queststatus");

            migrationBuilder.DropTable(
                name: "quickslotkeymapped");

            migrationBuilder.DropTable(
                name: "reactordrops");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "responses");

            migrationBuilder.DropTable(
                name: "rings");

            migrationBuilder.DropTable(
                name: "savedlocations");

            migrationBuilder.DropTable(
                name: "server_queue");

            migrationBuilder.DropTable(
                name: "shopitems");

            migrationBuilder.DropTable(
                name: "shops");

            migrationBuilder.DropTable(
                name: "skillmacros");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "specialcashitems");

            migrationBuilder.DropTable(
                name: "storages");

            migrationBuilder.DropTable(
                name: "trocklocations");

            migrationBuilder.DropTable(
                name: "wishlists");

            migrationBuilder.DropTable(
                name: "worldtransfers");

            migrationBuilder.DropTable(
                name: "dueypackages");

            migrationBuilder.DropTable(
                name: "pets");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "characters");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BanLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<int>(type: "tinyint", nullable: false),
                    ReasonDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_bindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValueSql: "''"),
                    MAC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    HWID = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValueSql: "''"),
                    LastActiveTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false, defaultValueSql: "''"),
                    password = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, defaultValueSql: "''"),
                    pin = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValueSql: "''"),
                    pic = table.Column<string>(type: "TEXT", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    lastlogin = table.Column<DateTimeOffset>(type: "timestamp", nullable: true),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    birthday = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "'2005-05-11'"),
                    nxCredit = table.Column<int>(type: "int", nullable: true),
                    maplePoint = table.Column<int>(type: "int", nullable: true),
                    nxPrepaid = table.Column<int>(type: "int", nullable: true),
                    characterslots = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'3'"),
                    gender = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'10'"),
                    tos = table.Column<bool>(type: "INTEGER", nullable: false),
                    gmlevel = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    nick = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    language = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'2'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alliance",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    capacity = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "'2'"),
                    notice = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValueSql: "''"),
                    rank1 = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'Master'"),
                    rank2 = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3 = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank4 = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'Member'"),
                    rank5 = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'Member'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "area_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    area = table.Column<int>(type: "int", nullable: false),
                    info = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bbs_replies",
                columns: table => new
                {
                    replyid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    threadid = table.Column<int>(type: "int", nullable: false),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "TEXT", maxLength: 26, nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.replyid);
                });

            migrationBuilder.CreateTable(
                name: "bbs_threads",
                columns: table => new
                {
                    threadid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    postercid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 26, nullable: false, defaultValueSql: "''"),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    icon = table.Column<short>(type: "smallint", nullable: false),
                    replycount = table.Column<short>(type: "smallint", nullable: false),
                    startpost = table.Column<string>(type: "text", nullable: false),
                    guildid = table.Column<int>(type: "int", nullable: false),
                    localthreadid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.threadid);
                });

            migrationBuilder.CreateTable(
                name: "bosslog_daily",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    bosstype = table.Column<string>(type: "text", nullable: false),
                    attempttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bosslog_weekly",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    bosstype = table.Column<string>(type: "text", nullable: false),
                    attempttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "buddies",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    buddyid = table.Column<int>(type: "int", nullable: false),
                    pending = table.Column<sbyte>(type: "tinyint", nullable: false),
                    group = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cdk_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    Expiration = table.Column<long>(type: "bigint", nullable: false),
                    MaxCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdk_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'4000000'"),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdk_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    RecipientTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "characterexplogs",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    world_exp_rate = table.Column<float>(type: "int", nullable: false),
                    exp_coupon = table.Column<float>(type: "int", nullable: false),
                    gained_exp = table.Column<long>(type: "bigint", nullable: false),
                    current_exp = table.Column<int>(type: "int", nullable: false),
                    exp_gain_time = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    world = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false, defaultValueSql: "''"),
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
                    sp = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, defaultValueSql: "'0,0,0,0,0,0,0,0,0,0'"),
                    map = table.Column<int>(type: "int", nullable: false),
                    spawnpoint = table.Column<int>(type: "int", nullable: false),
                    buddyCapacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'25'"),
                    createdate = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    reborns = table.Column<int>(type: "int", nullable: false),
                    PQPoints = table.Column<int>(type: "int", nullable: false),
                    dataString = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, defaultValueSql: "''"),
                    lastLogoutTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    lastExpGainTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    partySearch = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    jailexpire = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cooldowns",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    SkillID = table.Column<int>(type: "int", nullable: false),
                    length = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drop_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                });

            migrationBuilder.CreateTable(
                name: "drop_data_global",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    continent = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'-1'"),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    minimum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    maximum_quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    questid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    comments = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dueypackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Mesos = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    TimeStamp = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "'2015-01-01 05:00:00'"),
                    Message = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Checked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Type = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PackageId);
                });

            migrationBuilder.CreateTable(
                name: "eventstats",
                columns: table => new
                {
                    characterid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false, defaultValueSql: "'0'", comment: "0"),
                    info = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.characterid);
                });

            migrationBuilder.CreateTable(
                name: "family_entitlement",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    entitlementid = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fredstorage",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cid = table.Column<int>(type: "int", nullable: false),
                    daynotes = table.Column<int>(type: "int", nullable: false),
                    meso = table.Column<int>(type: "int", nullable: false),
                    itemMeso = table.Column<int>(type: "bigint", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gachapon_pool",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NpcId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gachapon_pool_item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gachapon_pool_level_chance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Chance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "tinytext", nullable: false),
                    sn = table.Column<int>(type: "int", nullable: false),
                    ringSourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    guildid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    leader = table.Column<int>(type: "int", nullable: false),
                    GP = table.Column<int>(type: "int", nullable: false),
                    logo = table.Column<int>(type: "int", nullable: false),
                    logoColor = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    rank1title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, defaultValueSql: "'Master'"),
                    rank2title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, defaultValueSql: "'Jr. Master'"),
                    rank3title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank4title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    rank5title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, defaultValueSql: "'Member'"),
                    capacity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'10'"),
                    logoBG = table.Column<int>(type: "int", nullable: false),
                    logoBGColor = table.Column<short>(type: "smallint", nullable: false),
                    notice = table.Column<string>(type: "TEXT", maxLength: 101, nullable: true),
                    signature = table.Column<long>(type: "int", nullable: false),
                    allianceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.guildid);
                });

            migrationBuilder.CreateTable(
                name: "hwidaccounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "INTEGER", nullable: false),
                    hwid = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    relevance = table.Column<sbyte>(type: "tinyint", nullable: false),
                    expiresat = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.accountid, x.hwid });
                });

            migrationBuilder.CreateTable(
                name: "hwidbans",
                columns: table => new
                {
                    hwidbanid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    hwid = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.hwidbanid);
                });

            migrationBuilder.CreateTable(
                name: "inventoryequipment",
                columns: table => new
                {
                    inventoryequipmentid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    inventoryitemid = table.Column<int>(type: "int", nullable: false),
                    upgradeslots = table.Column<int>(type: "int", nullable: false),
                    level = table.Column<byte>(type: "int", nullable: false),
                    str = table.Column<int>(type: "int", nullable: false),
                    dex = table.Column<int>(type: "int", nullable: false),
                    @int = table.Column<int>(name: "int", type: "int", nullable: false),
                    luk = table.Column<int>(type: "int", nullable: false),
                    hp = table.Column<int>(type: "int", nullable: false),
                    mp = table.Column<int>(type: "int", nullable: false),
                    watk = table.Column<int>(type: "int", nullable: false),
                    matk = table.Column<int>(type: "int", nullable: false),
                    wdef = table.Column<int>(type: "int", nullable: false),
                    mdef = table.Column<int>(type: "int", nullable: false),
                    acc = table.Column<int>(type: "int", nullable: false),
                    avoid = table.Column<int>(type: "int", nullable: false),
                    hands = table.Column<int>(type: "int", nullable: false),
                    speed = table.Column<int>(type: "int", nullable: false),
                    jump = table.Column<int>(type: "int", nullable: false),
                    locked = table.Column<int>(type: "int", nullable: false),
                    vicious = table.Column<int>(type: "int", nullable: false),
                    itemlevel = table.Column<byte>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    itemexp = table.Column<int>(type: "int", nullable: false),
                    ringid = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventoryequipmentid);
                });

            migrationBuilder.CreateTable(
                name: "inventoryitems",
                columns: table => new
                {
                    inventoryitemid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<byte>(type: "tinyint", nullable: false),
                    characterid = table.Column<int>(type: "int", nullable: true),
                    accountid = table.Column<int>(type: "int", nullable: true),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    inventorytype = table.Column<sbyte>(type: "int", nullable: false),
                    position = table.Column<short>(type: "int", nullable: false),
                    quantity = table.Column<short>(type: "int", nullable: false),
                    owner = table.Column<string>(type: "tinytext", nullable: false),
                    petid = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    flag = table.Column<short>(type: "int", nullable: false),
                    expiration = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "TEXT", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventoryitemid);
                });

            migrationBuilder.CreateTable(
                name: "ipbans",
                columns: table => new
                {
                    ipbanid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ip = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false, defaultValueSql: "''"),
                    aid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ipbanid);
                });

            migrationBuilder.CreateTable(
                name: "keymap",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    key = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "macbans",
                columns: table => new
                {
                    macbanid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    mac = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    aid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macbanid);
                });

            migrationBuilder.CreateTable(
                name: "macfilters",
                columns: table => new
                {
                    macfilterid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    filter = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macfilterid);
                });

            migrationBuilder.CreateTable(
                name: "makercreatedata",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    req_level = table.Column<short>(type: "smallint", nullable: false),
                    req_maker_level = table.Column<short>(type: "smallint", nullable: false),
                    req_meso = table.Column<int>(type: "int", nullable: false),
                    req_item = table.Column<int>(type: "int", nullable: false),
                    req_equip = table.Column<int>(type: "int", nullable: false),
                    catalyst = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<short>(type: "smallint", nullable: false),
                    tuc = table.Column<sbyte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.itemid });
                });

            migrationBuilder.CreateTable(
                name: "makerreagentdata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stat = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.itemid);
                });

            migrationBuilder.CreateTable(
                name: "makerrecipedata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "INTEGER", nullable: false),
                    req_item = table.Column<int>(type: "int", nullable: false),
                    count = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.itemid, x.req_item });
                });

            migrationBuilder.CreateTable(
                name: "makerrewarddata",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "INTEGER", nullable: false),
                    rewardid = table.Column<int>(type: "INTEGER", nullable: false),
                    quantity = table.Column<short>(type: "smallint", nullable: false),
                    prob = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'100'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.itemid, x.rewardid });
                });

            migrationBuilder.CreateTable(
                name: "marriages",
                columns: table => new
                {
                    marriageid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    husbandid = table.Column<int>(type: "int", nullable: false),
                    wifeid = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Time0 = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Time1 = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Time2 = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RingSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    EngagementItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.marriageid);
                });

            migrationBuilder.CreateTable(
                name: "medalmaps",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    queststatusid = table.Column<int>(type: "int", nullable: false),
                    mapid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monsterbook",
                columns: table => new
                {
                    charid = table.Column<int>(type: "INTEGER", nullable: false),
                    cardid = table.Column<int>(type: "INTEGER", nullable: false),
                    level = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monsterbook", x => new { x.cardid, x.charid });
                });

            migrationBuilder.CreateTable(
                name: "monstercarddata",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cardid = table.Column<int>(type: "int", nullable: false),
                    mobid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mts_cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mts_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    owner = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false, defaultValueSql: "''"),
                    sellername = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    sell_ends = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    transfer = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    vicious = table.Column<int>(type: "int", nullable: false),
                    flag = table.Column<int>(type: "int", nullable: false),
                    expiration = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'"),
                    giftFrom = table.Column<string>(type: "TEXT", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "namechanges",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    old = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    @new = table.Column<string>(name: "new", type: "TEXT", maxLength: 13, nullable: false),
                    requestTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completionTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "newyear",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    senderid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    receiverid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    message = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false, defaultValueSql: "''"),
                    senderdiscard = table.Column<bool>(type: "INTEGER", nullable: false),
                    receiverdiscard = table.Column<bool>(type: "INTEGER", nullable: false),
                    received = table.Column<bool>(type: "INTEGER", nullable: false),
                    timesent = table.Column<long>(type: "bigint", nullable: false),
                    timereceived = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    toId = table.Column<int>(type: "int", nullable: false),
                    fromId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    fame = table.Column<int>(type: "int", nullable: false),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nxcoupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    couponid = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<int>(type: "int", nullable: false),
                    activeday = table.Column<int>(type: "int", nullable: false),
                    starthour = table.Column<int>(type: "int", nullable: false),
                    endhour = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pets",
                columns: table => new
                {
                    petid = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    level = table.Column<int>(type: "int", nullable: false),
                    closeness = table.Column<int>(type: "int", nullable: false),
                    fullness = table.Column<int>(type: "int", nullable: false),
                    summoned = table.Column<bool>(type: "INTEGER", nullable: false),
                    flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.petid);
                });

            migrationBuilder.CreateTable(
                name: "playerdiseases",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    disease = table.Column<int>(type: "int", nullable: false),
                    mobskillid = table.Column<int>(type: "int", nullable: false),
                    mobskilllv = table.Column<int>(type: "int", nullable: false),
                    length = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playernpcs",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "playernpcs_equip",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    npcid = table.Column<int>(type: "int", nullable: false),
                    equipid = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    equippos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playernpcs_field",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    world = table.Column<int>(type: "int", nullable: false),
                    map = table.Column<int>(type: "int", nullable: false),
                    step = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    podium = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plife",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    world = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'"),
                    map = table.Column<int>(type: "int", nullable: false),
                    life = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false, defaultValueSql: "'n'"),
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
                });

            migrationBuilder.CreateTable(
                name: "questactions",
                columns: table => new
                {
                    questactionid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    questid = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    data = table.Column<byte[]>(type: "blob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.questactionid);
                });

            migrationBuilder.CreateTable(
                name: "questprogress",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    queststatusid = table.Column<int>(type: "int", nullable: false),
                    progressid = table.Column<int>(type: "int", nullable: false),
                    progress = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false, defaultValueSql: "''")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questrequirements",
                columns: table => new
                {
                    questrequirementid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    questid = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    data = table.Column<byte[]>(type: "blob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.questrequirementid);
                });

            migrationBuilder.CreateTable(
                name: "queststatus",
                columns: table => new
                {
                    queststatusid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    quest = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    time = table.Column<int>(type: "int", nullable: false),
                    expires = table.Column<long>(type: "bigint", nullable: false),
                    forfeited = table.Column<int>(type: "int", nullable: false),
                    completed = table.Column<int>(type: "int", nullable: false),
                    info = table.Column<sbyte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.queststatusid);
                });

            migrationBuilder.CreateTable(
                name: "reactordrops",
                columns: table => new
                {
                    reactordropid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    reactorid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    chance = table.Column<int>(type: "int", nullable: false),
                    questid = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.reactordropid);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    reporttime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    reporterid = table.Column<int>(type: "int", nullable: false),
                    victimid = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<sbyte>(type: "tinyint", nullable: false),
                    chatlog = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "responses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    chat = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rings",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    ringId1 = table.Column<long>(type: "bigint", nullable: false),
                    ringId2 = table.Column<long>(type: "bigint", nullable: false),
                    characterId1 = table.Column<int>(type: "int", nullable: false),
                    characterId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "savedlocations",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    locationtype = table.Column<string>(type: "text", nullable: false),
                    map = table.Column<int>(type: "int", nullable: false),
                    portal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "server_queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    value = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    createTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shopitems",
                columns: table => new
                {
                    shopitemid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    shopid = table.Column<int>(type: "int", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    pitch = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false, comment: "sort is an arbitrary field designed to give leeway when modifying shops. The lowest number is 104 and it increments by 4 for each item to allow decent space for swapping/inserting/removing items.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopitemid);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    shopid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    npcid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.shopid);
                });

            migrationBuilder.CreateTable(
                name: "skillmacros",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'"),
                    skill1 = table.Column<int>(type: "int", nullable: false),
                    skill2 = table.Column<int>(type: "int", nullable: false),
                    skill3 = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    shout = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialcashitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sn = table.Column<int>(type: "int", nullable: false),
                    modifier = table.Column<int>(type: "int", nullable: false, comment: "1024 is add/remove"),
                    info = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "storages",
                columns: table => new
                {
                    storageid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    slots = table.Column<int>(type: "int", nullable: false),
                    meso = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.storageid);
                });

            migrationBuilder.CreateTable(
                name: "trocklocations",
                columns: table => new
                {
                    trockid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    mapid = table.Column<int>(type: "int", nullable: false),
                    vip = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.trockid);
                });

            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    charid = table.Column<int>(type: "int", nullable: false),
                    sn = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quickslotkeymapped",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "INTEGER", nullable: false),
                    keymap = table.Column<long>(type: "bigint", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "famelog",
                columns: table => new
                {
                    famelogid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    characterid_to = table.Column<int>(type: "int", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "family_character",
                columns: table => new
                {
                    cid = table.Column<int>(type: "INTEGER", nullable: false),
                    familyid = table.Column<int>(type: "int", nullable: false),
                    seniorid = table.Column<int>(type: "int", nullable: false),
                    reputation = table.Column<int>(type: "int", nullable: false),
                    todaysrep = table.Column<int>(type: "int", nullable: false),
                    totalreputation = table.Column<int>(type: "int", nullable: false),
                    reptosenior = table.Column<int>(type: "int", nullable: false),
                    precepts = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    skillid = table.Column<int>(type: "int", nullable: false),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    skilllevel = table.Column<int>(type: "int", nullable: false),
                    masterlevel = table.Column<int>(type: "int", nullable: false),
                    expiration = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'-1'")
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
                });

            migrationBuilder.CreateTable(
                name: "dueyitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    inventoryitemid = table.Column<int>(type: "int", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "petignores",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    petid = table.Column<long>(type: "bigint", nullable: false),
                    itemid = table.Column<int>(type: "int", nullable: false)
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
                name: "id1",
                table: "characters",
                columns: new[] { "id", "accountid", "world" });

            migrationBuilder.CreateIndex(
                name: "ranking1",
                table: "characters",
                columns: new[] { "level", "exp" });

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
                name: "mobid",
                table: "drop_data",
                column: "dropperid");

            migrationBuilder.CreateIndex(
                name: "mobid1",
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
                name: "INVENTORYITEMID1",
                table: "inventoryequipment",
                column: "inventoryitemid");

            migrationBuilder.CreateIndex(
                name: "idx_inv_charId",
                table: "inventoryitems",
                column: "characterid");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_ban");

            migrationBuilder.DropTable(
                name: "account_bindings");

            migrationBuilder.DropTable(
                name: "alliance");

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
                name: "cdk_codes");

            migrationBuilder.DropTable(
                name: "cdk_items");

            migrationBuilder.DropTable(
                name: "cdk_records");

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
                name: "gachapon_pool");

            migrationBuilder.DropTable(
                name: "gachapon_pool_item");

            migrationBuilder.DropTable(
                name: "gachapon_pool_level_chance");

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

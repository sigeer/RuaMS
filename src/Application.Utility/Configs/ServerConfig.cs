namespace Application.Utility.Configs;

public class ServerConfig
{
    public string DB_CONNECTIONSTRING;

    public bool ENABLE_OPENAPI;

    //Login Configuration
    public int WORLDS;
    public int WLDLIST_SIZE;
    public int CHANNEL_SIZE;
    public int CHANNEL_LOAD;
    public int CHANNEL_LOCKS;

    public long RESPAWN_INTERVAL;
    public long PURGING_INTERVAL;
    public long RANKING_INTERVAL;
    public long COUPON_INTERVAL;
    public long UPDATE_INTERVAL;

    public bool ENABLE_PIC;
    public bool ENABLE_PIN;

    public int BYPASS_PIC_EXPIRATION;
    public int BYPASS_PIN_EXPIRATION;

    public bool AUTOMATIC_REGISTER;
    public bool COLLECTIVE_CHARSLOT;
    public bool DETERRED_MULTICLIENT;

    //Besides blocking logging in with several client sessions on the same machine, this also blocks suspicious login attempts for players that tries to login on an account using several diferent remote addresses.

    //Multiclient Coordinator Configuration
    public int MAX_ALLOWED_ACCOUNT_HWID;
    public int MAX_ACCOUNT_LOGIN_ATTEMPT;
    public int LOGIN_ATTEMPT_DURATION;

    //Ip Configuration
    public string HOST;
    public string LANHOST;
    public string LOCALHOST;

    //Server Flags
    public bool USE_CUSTOM_KEYSET;
    public bool USE_DEBUG;
    public bool USE_DEBUG_SHOW_INFO_EQPEXP;
    public bool USE_DEBUG_SHOW_RCVD_PACKET;
    public bool USE_DEBUG_SHOW_RCVD_MVLIFE;
    public bool USE_DEBUG_SHOW_PACKET;
    public bool USE_SUPPLY_RATE_COUPONS;
    public bool USE_IP_VALIDATION;
    public bool USE_CHARACTER_ACCOUNT_CHECK;

    public bool USE_MAXRANGE;
    public bool USE_MAXRANGE_ECHO_OF_HERO;
    public bool USE_MTS;
    public bool USE_CPQ;
    public bool USE_AUTOHIDE_GM;
    public bool USE_FAMILY_SYSTEM;
    public bool USE_DUEY;
    public bool USE_RANDOMIZE_HPMP_GAIN;
    public bool USE_STORAGE_ITEM_SORT;
    public bool USE_ITEM_SORT;
    public bool USE_ITEM_SORT_BY_NAME;
    public bool USE_PARTY_FOR_STARTERS;
    public bool USE_AUTOASSIGN_STARTERS_AP;
    public bool USE_AUTOASSIGN_SECONDARY_CAP;
    public bool USE_STARTING_AP_4;
    public bool USE_AUTOBAN;
    public bool USE_AUTOBAN_LOG;
    public bool USE_EXP_GAIN_LOG;
    public bool USE_AUTOSAVE;
    public bool USE_SERVER_AUTOASSIGNER;
    public bool USE_REFRESH_RANK_MOVE;
    public bool USE_ENFORCE_ADMIN_ACCOUNT;
    public bool USE_ENFORCE_NOVICE_EXPRATE;
    public bool USE_ENFORCE_HPMP_SWAP;
    public bool USE_ENFORCE_MOB_LEVEL_RANGE;
    public bool USE_ENFORCE_JOB_LEVEL_RANGE;
    public bool USE_ENFORCE_JOB_SP_RANGE;
    public bool USE_ENFORCE_ITEM_SUGGESTION;
    public bool USE_ENFORCE_UNMERCHABLE_CASH;
    public bool USE_ENFORCE_UNMERCHABLE_PET;
    public bool USE_ENFORCE_MERCHANT_SAVE;
    public bool USE_ENFORCE_MDOOR_POSITION;
    public bool USE_SPAWN_CLEAN_MDOOR;
    public bool USE_SPAWN_RELEVANT_LOOT;
    public bool USE_ERASE_PERMIT_ON_OPENSHOP;
    public bool USE_ERASE_UNTRADEABLE_DROP;
    public bool USE_ERASE_PET_ON_EXPIRATION;
    public bool USE_BUFF_MOST_SIGNIFICANT;
    public bool USE_BUFF_EVERLASTING;
    public bool USE_MULTIPLE_SAME_EQUIP_DROP;
    public bool USE_ENABLE_FULL_RESPAWN;
    public bool USE_ENABLE_CHAT_LOG;
    public bool USE_MAP_OWNERSHIP_SYSTEM;
    public bool USE_FISHING_SYSTEM;
    public bool USE_NPCS_SCRIPTABLE;

    //Events/PQs Configuration
    public bool USE_OLD_GMS_STYLED_PQ_NPCS;
    public bool USE_ENABLE_SOLO_EXPEDITIONS;
    public bool USE_ENABLE_DAILY_EXPEDITIONS;
    public bool USE_ENABLE_RECALL_EVENT;

    //Announcement Configuration
    public bool USE_ANNOUNCE_SHOPITEMSOLD;
    public bool USE_ANNOUNCE_CHANGEJOB;
    public bool USE_ANNOUNCE_NX_COUPON_LOOT;

    //Cash Shop Configuration
    public bool USE_JOINT_CASHSHOP_INVENTORY;
    public bool USE_CLEAR_OUTDATED_COUPONS;
    public bool ALLOW_CASHSHOP_NAME_CHANGE;

    //Maker Configuration
    public bool USE_MAKER_PERMISSIVE_ATKUP;
    public bool USE_MAKER_FEE_HEURISTICS;

    //Custom Configuration
    public bool USE_ENABLE_CUSTOM_NPC_SCRIPT;
    public bool USE_STARTER_MERGE;

    //Commands Configuration
    public bool BLOCK_GENERATE_CASH_ITEM;
    public bool USE_WHOLE_SERVER_RANKING;

    public double EQUIP_EXP_RATE;
    public double PQ_BONUS_EXP_RATE;

    public byte EXP_SPLIT_LEVEL_INTERVAL;
    public byte EXP_SPLIT_LEECH_INTERVAL;
    public float EXP_SPLIT_MVP_MOD;
    public float EXP_SPLIT_COMMON_MOD;
    public float PARTY_BONUS_EXP_RATE;

    //Miscellaneous Configuration
    public byte MAX_MONITORED_BUFFSTATS;
    public int MAX_AP;
    public int MAX_EVENT_LEVELS;
    public long BLOCK_NPC_RACE_CONDT;
    public int TOT_MOB_QUEST_REQUIREMENT;
    public int MOB_REACTOR_REFRESH_TIME;
    public int PARTY_SEARCH_REENTRY_LIMIT;
    public long NAME_CHANGE_COOLDOWN;
    public long WORLD_TRANSFER_COOLDOWN;//Cooldown for world tranfers, default is same as name change (30 days).
    public bool INSTANT_NAME_CHANGE;

    //Dangling Items/Locks Configuration
    public int ITEM_EXPIRE_TIME;
    public int KITE_EXPIRE_TIME;
    public int ITEM_MONITOR_TIME;
    public int LOCK_MONITOR_TIME;

    //Map Monitor Configuration
    public int ITEM_EXPIRE_CHECK;
    public int ITEM_LIMIT_ON_MAP;
    public int MAP_VISITED_SIZE;
    public int MAP_DAMAGE_OVERTIME_INTERVAL;
    public int MAP_DAMAGE_OVERTIME_COUNT;

    //Channel Mob Disease Monitor Configuration
    public int MOB_STATUS_MONITOR_PROC;
    public int MOB_STATUS_MONITOR_LIFE;
    public int MOB_STATUS_AGGRO_PERSISTENCE;
    public int MOB_STATUS_AGGRO_INTERVAL;
    public bool USE_AUTOAGGRO_NEARBY;

    //Some Gameplay Enhancing Configurations
    //Scroll Configuration
    public bool USE_PERFECT_GM_SCROLL;
    public bool USE_PERFECT_SCROLLING;
    public bool USE_ENHANCED_CHSCROLL;
    public bool USE_ENHANCED_CRAFTING;
    public int SCROLL_CHANCE_ROLLS;
    public int CHSCROLL_STAT_RATE;
    public int CHSCROLL_STAT_RANGE;

    //Beginner Skills Configuration
    public bool USE_ULTRA_NIMBLE_FEET;
    public bool USE_ULTRA_RECOVERY;
    public bool USE_ULTRA_THREE_SNAILS;

    //Other Skills Configuration
    public bool USE_FULL_ARAN_SKILLSET;
    public bool USE_FAST_REUSE_HERO_WILL;
    public bool USE_ANTI_IMMUNITY_CRASH;
    public bool USE_UNDISPEL_HOLY_SHIELD;
    public bool USE_FULL_HOLY_SYMBOL;

    //Character Configuration
    public bool USE_ADD_SLOTS_BY_LEVEL;
    public bool USE_ADD_RATES_BY_LEVEL;
    public bool USE_STACK_COUPON_RATES;
    public bool USE_PERFECT_PITCH;

    //Quest Configuration
    public bool USE_QUEST_RATE;

    //Quest Points Configuration
    public int QUEST_POINT_REPEATABLE_INTERVAL;
    public int QUEST_POINT_REQUIREMENT;
    public int QUEST_POINT_PER_QUEST_COMPLETE;
    public int QUEST_POINT_PER_EVENT_CLEAR;

    //Guild Configuration
    public int CREATE_GUILD_MIN_PARTNERS;
    public int CREATE_GUILD_COST;
    public int CHANGE_EMBLEM_COST;
    public int EXPAND_GUILD_BASE_COST;
    public int EXPAND_GUILD_TIER_COST;
    public int EXPAND_GUILD_MAX_COST;

    //Family Configuration
    public int FAMILY_REP_PER_KILL;
    public int FAMILY_REP_PER_BOSS_KILL;
    public int FAMILY_REP_PER_LEVELUP;
    public int FAMILY_MAX_GENERATIONS;

    //Equipment Configuration
    public bool USE_EQUIPMNT_LVLUP_SLOTS;
    public bool USE_EQUIPMNT_LVLUP_POWER;
    public bool USE_EQUIPMNT_LVLUP_CASH;
    public int MAX_EQUIPMNT_LVLUP_STAT_UP;
    public int MAX_EQUIPMNT_STAT;
    public int USE_EQUIPMNT_LVLUP;

    //Map-Chair Configuration
    public bool USE_CHAIR_EXTRAHEAL;
    public byte CHAIR_EXTRA_HEAL_MULTIPLIER;
    public int CHAIR_EXTRA_HEAL_MAX_DELAY;

    //Player NPC Configuration
    public int PLAYERNPC_INITIAL_X;
    public int PLAYERNPC_INITIAL_Y;
    public int PLAYERNPC_AREA_X;
    public int PLAYERNPC_AREA_Y;
    public int PLAYERNPC_AREA_STEPS;
    public bool PLAYERNPC_ORGANIZE_AREA;
    public bool PLAYERNPC_AUTODEPLOY;

    //Pet Auto-Pot Configuration
    public bool USE_COMPULSORY_AUTOPOT;
    public bool USE_EQUIPS_ON_AUTOPOT;
    public double PET_AUTOHP_RATIO;
    public double PET_AUTOMP_RATIO;

    //Pet & Mount Configuration
    public byte PET_EXHAUST_COUNT;
    public byte MOUNT_EXHAUST_COUNT;

    //Pet Hunger Configuration
    public bool PETS_NEVER_HUNGRY;
    public bool GM_PETS_NEVER_HUNGRY;

    //Event Configuration
    public int EVENT_MAX_GUILD_QUEUE;
    public long EVENT_LOBBY_DELAY;

    //Dojo Configuration
    public bool USE_FAST_DOJO_UPGRADE;
    public bool USE_DEADLY_DOJO;
    public int DOJO_ENERGY_ATK;
    public int DOJO_ENERGY_DMG;

    //Wedding Configuration
    public int WEDDING_RESERVATION_DELAY;
    public int WEDDING_RESERVATION_TIMEOUT;
    public int WEDDING_RESERVATION_INTERVAL;
    public int WEDDING_BLESS_EXP;
    public int WEDDING_GIFT_LIMIT;
    public bool WEDDING_BLESSER_SHOWFX;

    // Login timeout by shavit
    public long TIMEOUT_DURATION;

    //GM Security Configuration
    public int MINIMUM_GM_LEVEL_TO_TRADE;
    public int MINIMUM_GM_LEVEL_TO_USE_STORAGE;
    public int MINIMUM_GM_LEVEL_TO_USE_DUEY;
    public int MINIMUM_GM_LEVEL_TO_DROP;

    //Custom NPC overrides. List of NPC IDs.
    public Dictionary<string, string> NPCS_SCRIPTABLE = new ();
}

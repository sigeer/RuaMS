namespace Application.Shared.Constants.Map;

public class MapId
{
    // Special
    public const int NONE = 999999999;
    public const int GM_MAP = 180000000;
    public const int JAIL = 300000012; // "Cellar: Camp Conference Room"
    public const int DEVELOPERS_HQ = 777777777;

    // Misc
    public const int ORBIS_TOWER_BOTTOM = 200082300;
    public const int INTERNET_CAFE = 193000000;
    public const int CRIMSONWOOD_VALLEY_1 = 610020000;
    public const int CRIMSONWOOD_VALLEY_2 = 610020001;
    public const int HENESYS_PQ = 910010000;
    public const int ORIGIN_OF_CLOCKTOWER = 220080001;
    public const int CAVE_OF_PIANUS = 230040420;
    public const int GUILD_HQ = 200000301;
    public const int FM_ENTRANCE = 910000000;

    // Beginner
    public static int MUSHROOM_TOWN = 10000;

    // Town
    public const int SOUTHPERRY = 2000000;
    public const int AMHERST = 1000000;
    public const int HENESYS = 100000000;
    public const int ELLINIA = 101000000;
    public const int PERION = 102000000;
    public const int KERNING_CITY = 103000000;
    public const int LITH_HARBOUR = 104000000;
    public const int SLEEPYWOOD = 105040300;
    public const int MUSHROOM_KINGDOM = 106020000;
    public const int FLORINA_BEACH = 110000000;
    public const int EREVE = 130000000;
    public const int KERNING_SQUARE = 103040000;
    public const int RIEN = 140000000;
    public const int ORBIS = 200000000;
    public const int EL_NATH = 211000000;
    public const int LUDIBRIUM = 220000000;
    public const int AQUARIUM = 230000000;
    public const int LEAFRE = 240000000;
    public const int NEO_CITY = 240070000;
    public const int MU_LUNG = 250000000;
    public const int HERB_TOWN = 251000000;
    public const int OMEGA_SECTOR = 221000000;
    public const int KOREAN_FOLK_TOWN = 222000000;
    public const int ARIANT = 260000000;
    public const int MAGATIA = 261000000;
    public const int TEMPLE_OF_TIME = 270000100;
    public const int ELLIN_FOREST = 300000000;
    public const int SINGAPORE = 540000000;
    public const int BOAT_QUAY_TOWN = 541000000;
    public const int KAMPUNG_VILLAGE = 551000000;
    public const int NEW_LEAF_CITY = 600000000;
    public const int MUSHROOM_SHRINE = 800000000;
    public const int SHOWA_TOWN = 801000000;
    public const int NAUTILUS_HARBOR = 120000000;
    public const int HAPPYVILLE = 209000000;

    public static int SHOWA_SPA_M = 809000101;
    public static int SHOWA_SPA_F = 809000201;

    private static int MAPLE_ISLAND_MIN = 0;
    private static int MAPLE_ISLAND_MAX = 2000001;

    public static bool isMapleIsland(int mapId)
    {
        return mapId >= MAPLE_ISLAND_MIN && mapId <= MAPLE_ISLAND_MAX;
    }

    // Travel
    // There are 10 of each of these travel maps in the files
    public const int FROM_LITH_TO_RIEN = 200090060;
    public const int FROM_RIEN_TO_LITH = 200090070;
    public const int DANGEROUS_FOREST = 140020300; // Rien docks
    public const int FROM_ELLINIA_TO_EREVE = 200090030;
    public const int SKY_FERRY = 130000210; // Ereve platform
    public const int FROM_EREVE_TO_ELLINIA = 200090031;
    public const int ELLINIA_SKY_FERRY = 101000400;
    public const int FROM_EREVE_TO_ORBIS = 200090021;
    public const int ORBIS_STATION = 200000161;
    public const int FROM_ORBIS_TO_EREVE = 200090020;

    // Aran
    public const int ARAN_TUTORIAL_START = 914000000;
    public const int ARAN_TUTORIAL_MAX = 914000500;
    public const int ARAN_INTRO = 140090000;
    private const int BURNING_FOREST_1 = 914000200;
    private const int BURNING_FOREST_2 = 914000210;
    private const int BURNING_FOREST_3 = 914000220;

    // Aran tutorial
    public static bool isGodlyStatMap(int mapId)
    {
        return mapId == BURNING_FOREST_1 || mapId == BURNING_FOREST_2 || mapId == BURNING_FOREST_3;
    }

    // Aran intro
    public const int ARAN_TUTO_1 = 914090010;
    public const int ARAN_TUTO_2 = 914090011;
    public const int ARAN_TUTO_3 = 914090012;
    public const int ARAN_TUTO_4 = 914090013;
    public const int ARAN_POLEARM = 914090100;
    public const int ARAN_MAHA = 914090200; // Black screen when warped to

    // Starting map
    public static int STARTING_MAP_NOBLESSE = 130030000;

    // Cygnus intro
    // These are the actual maps
    private static int CYGNUS_INTRO_LOCATION_MIN = 913040000;
    private static int CYGNUS_INTRO_LOCATION_MAX = 913040006;

    public static bool isCygnusIntro(int mapId)
    {
        return mapId >= CYGNUS_INTRO_LOCATION_MIN && mapId <= CYGNUS_INTRO_LOCATION_MAX;
    }

    // Cygnus intro video
    public const int CYGNUS_INTRO_LEAD = 913040100;
    public const int CYGNUS_INTRO_WARRIOR = 913040101;
    public const int CYGNUS_INTRO_BOWMAN = 913040102;
    public const int CYGNUS_INTRO_MAGE = 913040103;
    public const int CYGNUS_INTRO_PIRATE = 913040104;
    public const int CYGNUS_INTRO_THIEF = 913040105;
    public const int CYGNUS_INTRO_CONCLUSION = 913040106;

    // Event
    public const int EVENT_COCONUT_HARVEST = 109080000;
    public const int EVENT_OX_QUIZ = 109020001;
    public const int EVENT_PHYSICAL_FITNESS = 109040000;
    public const int EVENT_OLA_OLA_0 = 109030001;
    public const int EVENT_OLA_OLA_1 = 109030101;
    public const int EVENT_OLA_OLA_2 = 109030201;
    public const int EVENT_OLA_OLA_3 = 109030301;
    public const int EVENT_OLA_OLA_4 = 109030401;
    public const int EVENT_SNOWBALL = 109060000;
    public const int EVENT_FIND_THE_JEWEL = 109010000;
    public const int FITNESS_EVENT_LAST = 109040004;
    public const int OLA_EVENT_LAST_1 = 109030003;
    public const int OLA_EVENT_LAST_2 = 109030103;
    public const int WITCH_TOWER_ENTRANCE = 980040000;
    public const int EVENT_WINNER = 109050000;
    public const int EVENT_EXIT = 109050001;
    public const int EVENT_SNOWBALL_ENTRANCE = 109060001;

    private static int PHYSICAL_FITNESS_MIN = EVENT_PHYSICAL_FITNESS;
    private static int PHYSICAL_FITNESS_MAX = FITNESS_EVENT_LAST;

    public static bool isPhysicalFitness(int mapId)
    {
        return mapId >= PHYSICAL_FITNESS_MIN && mapId <= PHYSICAL_FITNESS_MAX;
    }

    private const int OLA_OLA_MIN = EVENT_OLA_OLA_0;
    private const int OLA_OLA_MAX = 109030403; // OLA_OLA_4 level 3

    public static bool isOlaOla(int mapId)
    {
        return mapId >= OLA_OLA_MIN && mapId <= OLA_OLA_MAX;
    }

    // Self lootable maps
    private const int HAPPYVILLE_TREE_MIN = 209000001;
    private const int HAPPYVILLE_TREE_MAX = 209000015;
    private const int GPQ_FOUNTAIN_MIN = 990000500;
    private const int GPQ_FOUNTAIN_MAX = 990000502;

    public static bool isSelfLootableOnly(int mapId)
    {
        return (mapId >= HAPPYVILLE_TREE_MIN && mapId <= HAPPYVILLE_TREE_MAX) ||
                (mapId >= GPQ_FOUNTAIN_MIN && mapId <= GPQ_FOUNTAIN_MAX);
    }

    // Dojo
    public const int DOJO_SOLO_BASE = 925020000;
    public const int DOJO_PARTY_BASE = 925030000;
    public const int DOJO_EXIT = 925020002;
    private const int DOJO_MIN = DOJO_SOLO_BASE;
    private const int DOJO_MAX = 925033804;
    private const int DOJO_PARTY_MIN = 925030100;
    public const int DOJO_PARTY_MAX = DOJO_MAX;

    public static bool isDojo(int mapId)
    {
        return mapId >= DOJO_MIN && mapId <= DOJO_MAX;
    }

    public static bool isPartyDojo(int mapId)
    {
        return mapId >= DOJO_PARTY_MIN && mapId <= DOJO_PARTY_MAX;
    }

    // Mini dungeon
    public const int ANT_TUNNEL_2 = 105050100;
    public const int CAVE_OF_MUSHROOMS_BASE = 105050101;
    public const int SLEEPY_DUNGEON_4 = 105040304;
    public const int GOLEMS_CASTLE_RUINS_BASE = 105040320;
    public const int SAHEL_2 = 260020600;
    public const int HILL_OF_SANDSTORMS_BASE = 260020630;
    public const int RAIN_FOREST_EAST_OF_HENESYS = 100020000;
    public const int HENESYS_PIG_FARM_BASE = 100020100;
    public const int COLD_CRADLE = 105090311;
    public const int DRAKES_BLUE_CAVE_BASE = 105090320;
    public const int EOS_TOWER_76TH_TO_90TH_FLOOR = 221023400;
    public const int DRUMMER_BUNNYS_LAIR_BASE = 221023401;
    public const int BATTLEFIELD_OF_FIRE_AND_WATER = 240020500;
    public const int ROUND_TABLE_OF_KENTAURUS_BASE = 240020512;
    public const int RESTORING_MEMORY_BASE = 240040800;
    public const int DESTROYED_DRAGON_NEST = 240040520;
    public const int NEWT_SECURED_ZONE_BASE = 240040900;
    public const int RED_NOSE_PIRATE_DEN_2 = 251010402;
    public const int PILLAGE_OF_TREASURE_ISLAND_BASE = 251010410;
    public const int LAB_AREA_C1 = 261020300;
    public const int CRITICAL_ERROR_BASE = 261020301;
    public const int FANTASY_THEME_PARK_3 = 551030000;
    public const int LONGEST_RIDE_ON_BYEBYE_STATION = 551030001;

    // Boss rush
    private const int BOSS_RUSH_MIN = 970030100;
    private const int BOSS_RUSH_MAX = 970042711;

    public static bool isBossRush(int mapId)
    {
        return mapId >= BOSS_RUSH_MIN && mapId <= BOSS_RUSH_MAX;
    }

    // ARPQ
    public const int ARPQ_LOBBY = 980010000;
    public const int ARPQ_ARENA_1 = 980010101;
    public const int ARPQ_ARENA_2 = 980010201;
    public const int ARPQ_ARENA_3 = 980010301;
    public const int ARPQ_KINGS_ROOM = 980010010;

    // Nett's pyramid
    public const int NETTS_PYRAMID = 926010001;
    public const int NETTS_PYRAMID_SOLO_BASE = 926010100;
    public const int NETTS_PYRAMID_PARTY_BASE = 926020100;
    private static int NETTS_PYRAMID_MIN = NETTS_PYRAMID_SOLO_BASE;
    private static int NETTS_PYRAMID_MAX = 926023500;

    public static bool isNettsPyramid(int mapId)
    {
        return mapId >= NETTS_PYRAMID_MIN && mapId <= NETTS_PYRAMID_MAX;
    }

    // Fishing
    private const int ON_THE_WAY_TO_THE_HARBOR = 120010000;
    private const int PIER_ON_THE_BEACH = 251000100;
    private const int PEACEFUL_SHIP = 541010110;

    public static bool isFishingArea(int mapId)
    {
        return mapId == ON_THE_WAY_TO_THE_HARBOR || mapId == PIER_ON_THE_BEACH || mapId == PEACEFUL_SHIP;
    }

    // Wedding
    public const int AMORIA = 680000000;
    public const int CHAPEL_WEDDING_ALTAR = 680000110;
    public const int CATHEDRAL_WEDDING_ALTAR = 680000210;
    public const int WEDDING_PHOTO = 680000300;
    public const int WEDDING_EXIT = 680000500;

    // Statue
    public const int HALL_OF_WARRIORS = 102000004; // Explorer
    public const int HALL_OF_MAGICIANS = 101000004;
    public const int HALL_OF_BOWMEN = 100000204;
    public const int HALL_OF_THIEVES = 103000008;
    public const int NAUTILUS_TRAINING_ROOM = 120000105;
    public const int KNIGHTS_CHAMBER = 130000100; // Cygnus
    public const int KNIGHTS_CHAMBER_2 = 130000110;
    public const int KNIGHTS_CHAMBER_3 = 130000120;
    public const int KNIGHTS_CHAMBER_LARGE = 130000101;
    public const int PALACE_OF_THE_MASTER = 140010110; // Aran

    // gm-goto
    public const int EXCAVATION_SITE = 990000000;
    public const int SOMEONE_ELSES_HOUSE = 100000005;
    public const int GRIFFEY_FOREST = 240020101;
    public const int MANONS_FOREST = 240020401;
    public const int HOLLOWED_GROUND = 682000001;
    public const int CURSED_SANCTUARY = 105090900;
    public const int DOOR_TO_ZAKUM = 211042300;
    public const int DRAGON_NEST_LEFT_BEHIND = 240040511;
    public const int HENESYS_PARK = 100000200;
    public const int ENTRANCE_TO_HORNTAILS_CAVE = 240050400;
    public const int FORGOTTEN_TWILIGHT = 270050000;
    public const int CRIMSONWOOD_KEEP = 610020006;
    public const int MU_LUNG_DOJO_HALL = 925020001;
    public const int EXCLUSIVE_TRAINING_CENTER = 970030000;
}

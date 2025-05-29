namespace Application.Shared.Constants.Item;



public class ItemId
{
    // Misc
    public const int PENDANT_OF_THE_SPIRIT = 1122017;
    public const int HEART_SHAPED_CHOCOLATE = 5110000;
    public const int HAPPY_BIRTHDAY = 2022153;
    public const int FISHING_CHAIR = 3011000;
    public const int MINI_GAME_BASE = 4080000;
    public const int MATCH_CARDS = 4080100;
    public const int MAGICAL_MITTEN = 1472063;
    public const int RPS_CERTIFICATE_BASE = 4031332;
    public const int GOLDEN_MAPLE_LEAF = 4000313;
    public const int PERFECT_PITCH = 4310000;
    public const int MAGIC_ROCK = 4006000;
    public const int GOLDEN_CHICKEN_EFFECT = 4290000;
    public const int BUMMER_EFFECT = 4290001;
    public const int ARPQ_SHIELD = 2022269;
    public const int ROARING_TIGER_MESSENGER = 5390006;

    public const int SKILLBOOK_MIN_ITEMID = 2280000;
    public const int SKILLBOOK_MAX_ITEMID = 2300000;  // exclusively

    public static bool isExpIncrease(int itemId)
    {
        return itemId >= 2022450 && itemId <= 2022452;
    }

    public static bool isRateCoupon(int itemId)
    {
        int itemType = itemId / 1000;
        return itemType == 5211 || itemType == 5360;
    }

    public static bool isMonsterCard(int itemId)
    {
        int itemType = itemId / 10000;
        return itemType == 238;
    }

    public static bool HasScript(int itemId)
    {
        return (itemId / 10000) == 243;
    }

    public static bool isPyramidBuff(int itemId)
    {
        return (itemId >= 2022585 && itemId <= 2022588) || (itemId >= 2022616 && itemId <= 2022617);
    }

    public static bool isDojoBuff(int itemId)
    {
        return itemId >= 2022359 && itemId <= 2022421;
    }

    // Potion
    public static int WHITE_POTION = 2000002;
    public static int BLUE_POTION = 2000003;
    public static int ORANGE_POTION = 2000001;
    public static int MANA_ELIXIR = 2000006;

    // HP/MP recovery
    public static int SORCERERS_POTION = 2022337;
    public static int RUSSELLONS_PILLS = 2022198;

    // Environment
    public static int RED_BEAN_PORRIDGE = 2022001;
    public static int SOFT_WHITE_BUN = 2022186;
    public static int AIR_BUBBLE = 2022040;

    // Chair
    public static int RELAXER = 3010000;
    private static int CHAIR_MIN = RELAXER;
    private static int CHAIR_MAX = FISHING_CHAIR;

    public static bool isChair(int itemId)
    {
        return itemId >= CHAIR_MIN && itemId <= CHAIR_MAX;
        // alt: return itemId / 10000 == 301;
    }

    // Throwing star
    public const int SUBI_THROWING_STARS = 2070000;
    public const int HWABI_THROWING_STARS = 2070007;
    public const int BALANCED_FURY = 2070018;
    public const int CRYSTAL_ILBI_THROWING_STARS = 2070016;
    private static int THROWING_STAR_MIN = SUBI_THROWING_STARS;
    private const int THROWING_STAR_MAX = 2070016;
    public const int DEVIL_RAIN_THROWING_STAR = 2070014;

    public static int[] allThrowingStarIds()
    {
        return Enumerable.Range(THROWING_STAR_MIN, THROWING_STAR_MAX - THROWING_STAR_MIN + 1).ToArray();
    }

    // Bullet
    public const int BULLET = 2330000;
    private const int BULLET_MIN = BULLET;
    private const int BULLET_MAX = 2330005;
    public const int BLAZE_CAPSULE = 2331000;
    public const int GLAZE_CAPSULE = 2332000;

    public static int[] allBulletIds()
    {
        return Enumerable.Range(BULLET_MIN, BULLET_MAX - BULLET_MIN + 1).ToArray();
    }

    // Starter
    public const int BEGINNERS_GUIDE = 4161001;
    public const int LEGENDS_GUIDE = 4161048;
    public const int NOBLESSE_GUIDE = 4161047;

    // Warrior
    public const int RED_HWARANG_SHIRT = 1040021;
    public const int BLACK_MARTIAL_ARTS_PANTS = 1060016;
    public const int MITHRIL_BATTLE_GRIEVES = 1072039;
    public const int GLADIUS = 1302008;
    public const int MITHRIL_POLE_ARM = 1442001;
    public const int MITHRIL_MAUL = 1422001;
    public const int FIREMANS_AXE = 1312005;
    public const int DARK_ENGRIT = 1051010;

    // Bowman
    public const int GREEN_HUNTERS_ARMOR = 1040067;
    public const int GREEN_HUNTRESS_ARMOR = 1041054;
    public const int GREEN_HUNTERS_PANTS = 1060056;
    public const int GREEN_HUNTRESS_PANTS = 1061050;
    public const int GREEN_HUNTER_BOOTS = 1072081;
    public const int RYDEN = 1452005;
    public const int MOUNTAIN_CROSSBOW = 1462000;

    // Magician
    public const int BLUE_WIZARD_ROBE = 1050003;
    public const int PURPLE_FAIRY_TOP = 1041041;
    public const int PURPLE_FAIRY_SKIRT = 1061034;
    public const int RED_MAGICSHOES = 1072075;
    public const int MITHRIL_WAND = 1372003;
    public const int CIRCLE_WINDED_STAFF = 1382017;

    // Thief
    public const int DARK_BROWN_STEALER = 1040057;
    public const int RED_STEAL = 1041047;
    public const int DARK_BROWN_STEALER_PANTS = 1060043;
    public const int RED_STEAL_PANTS = 1061043;
    public const int BRONZE_CHAIN_BOOTS = 1072032;
    public const int STEEL_GUARDS = 1472008;
    public const int REEF_CLAW = 1332012;

    // Pirate
    public const int BROWN_PAULIE_BOOTS = 1072294;
    public const int PRIME_HANDS = 1482004;
    public const int COLD_MIND = 1492004;
    public const int BROWN_POLLARD = 1052107;

    // Three snails
    public const int SNAIL_SHELL = 4000019;
    public const int BLUE_SNAIL_SHELL = 4000000;
    public const int RED_SNAIL_SHELL = 4000016;

    // Special scroll
    public const int COLD_PROTECTION_SCROLl = 2041058;
    public const int SPIKES_SCROLL = 2040727;
    public const int VEGAS_SPELL_10 = 5610000;
    public const int VEGAS_SPELL_60 = 5610001;
    public const int CHAOS_SCROll_60 = 2049100;
    public const int LIAR_TREE_SAP = 2049101;
    public const int MAPLE_SYRUP = 2049102;
    public const int WHITE_SCROLL = 2340000;
    public const int CLEAN_SLATE_1 = 2049000;
    public const int CLEAN_SLATE_3 = 2049001;
    public const int CLEAN_SLATE_5 = 2049002;
    public const int CLEAN_SLATE_20 = 2049003;
    public const int RING_STR_100_SCROLL = 2041100;
    public const int DRAGON_STONE_SCROLL = 2041200;
    public const int BELT_STR_100_SCROLL = 2041300;

    // Cure debuff
    public const int ALL_CURE_POTION = 2050004;
    public const int EYEDROP = 2050001;
    public const int TONIC = 2050002;
    public const int HOLY_WATER = 2050003;
    private const int DOJO_PARTY_ALL_CURE = 2022433;
    private const int CARNIVAL_PARTY_ALL_CURE = 2022163;
    public const int WHITE_ELIXIR = 2022544;

    public static bool isPartyAllCure(int itemId)
    {
        return itemId == DOJO_PARTY_ALL_CURE || itemId == CARNIVAL_PARTY_ALL_CURE;
    }

    // Special effect
    public const int PHARAOHS_BLESSING_1 = 2022585;
    public const int PHARAOHS_BLESSING_2 = 2022586;
    public const int PHARAOHS_BLESSING_3 = 2022587;
    public const int PHARAOHS_BLESSING_4 = 2022588;

    // Evolve pet
    public const int DRAGON_PET = 5000028;
    public const int ROBO_PET = 5000047;

    // Pet equip
    public const int MESO_MAGNET = 1812000;
    public const int ITEM_POUCH = 1812001;
    public const int ITEM_IGNORE = 1812007;

    public static bool isPet(int itemId)
    {
        return itemId / 1000 == 5000;
    }

    // Expirable pet
    public static int PET_SNAIL = 5000054;

    // Permanent pet
    private static int PERMA_PINK_BEAN = 5000060;
    private static int PERMA_KINO = 5000100;
    private static int PERMA_WHITE_TIGER = 5000101;
    private static int PERMA_MINI_YETI = 5000102;

    public static int[] getPermaPets()
    {
        return new int[] { PERMA_PINK_BEAN, PERMA_KINO, PERMA_WHITE_TIGER, PERMA_MINI_YETI };
    }

    // Maker
    public const int BASIC_MONSTER_CRYSTAL_1 = 4260000;
    public const int BASIC_MONSTER_CRYSTAL_2 = 4260001;
    public const int BASIC_MONSTER_CRYSTAL_3 = 4260002;
    public const int INTERMEDIATE_MONSTER_CRYSTAL_1 = 4260003;
    public const int INTERMEDIATE_MONSTER_CRYSTAL_2 = 4260004;
    public const int INTERMEDIATE_MONSTER_CRYSTAL_3 = 4260005;
    public const int ADVANCED_MONSTER_CRYSTAL_1 = 4260006;
    public const int ADVANCED_MONSTER_CRYSTAL_2 = 4260007;
    public const int ADVANCED_MONSTER_CRYSTAL_3 = 4260008;

    // NPC weather (PQ)
    public const int NPC_WEATHER_GROWLIE = 5120016; // Henesys PQ

    // Safety charm
    public const int SAFETY_CHARM = 5130000;
    public const int EASTER_BASKET = 4031283;
    public const int EASTER_CHARM = 4140903;

    // Engagement box
    public const int ENGAGEMENT_BOX_MOONSTONE = 2240000;
    public const int ENGAGEMENT_BOX_STAR = 2240001;
    public const int ENGAGEMENT_BOX_GOLDEN = 2240002;
    public const int ENGAGEMENT_BOX_SILVER = 2240003;
    public const int EMPTY_ENGAGEMENT_BOX_MOONSTONE = 4031357;
    public const int ENGAGEMENT_RING_MOONSTONE = 4031358;
    public const int EMPTY_ENGAGEMENT_BOX_STAR = 4031359;
    public const int ENGAGEMENT_RING_STAR = 4031360;
    public const int EMPTY_ENGAGEMENT_BOX_GOLDEN = 4031361;
    public const int ENGAGEMENT_RING_GOLDEN = 4031362;
    public const int EMPTY_ENGAGEMENT_BOX_SILVER = 4031363;
    public const int ENGAGEMENT_RING_SILVER = 4031364;

    public static bool isWeddingToken(int itemId)
    {
        return itemId >= ItemId.EMPTY_ENGAGEMENT_BOX_MOONSTONE && itemId <= ItemId.ENGAGEMENT_RING_SILVER;
    }

    // Wedding etc
    public const int PARENTS_BLESSING = 4031373;
    public const int OFFICIATORS_PERMISSION = 4031374;
    public const int ONYX_CHEST_FOR_COUPLE = 4031424;

    // Wedding ticket
    public const int NORMAL_WEDDING_TICKET_CATHEDRAL = 5251000;
    public const int NORMAL_WEDDING_TICKET_CHAPEL = 5251001;
    public const int PREMIUM_WEDDING_TICKET_CHAPEL = 5251002;
    public const int PREMIUM_WEDDING_TICKET_CATHEDRAL = 5251003;

    // Wedding reservation
    public const int PREMIUM_CATHEDRAL_RESERVATION_RECEIPT = 4031375;
    public const int PREMIUM_CHAPEL_RESERVATION_RECEIPT = 4031376;
    public const int NORMAL_CATHEDRAL_RESERVATION_RECEIPT = 4031480;
    public const int NORMAL_CHAPEL_RESERVATION_RECEIPT = 4031481;

    // Wedding invite
    public const int INVITATION_CHAPEL = 4031377;
    public const int INVITATION_CATHEDRAL = 4031395;
    public const int RECEIVED_INVITATION_CHAPEL = 4031406;
    public const int RECEIVED_INVITATION_CATHEDRAL = 4031407;

    public static int CARAT_RING_BASE = 1112300; // Unsure about math on this and the following one
    public static int CARAT_RING_BOX_BASE = 2240004;
    private static int CARAT_RING_BOX_MAX = 2240015;

    public static int ENGAGEMENT_BOX_MIN = ENGAGEMENT_BOX_MOONSTONE;
    public static int ENGAGEMENT_BOX_MAX = CARAT_RING_BOX_MAX;

    // Wedding ring
    public const int WEDDING_RING_MOONSTONE = 1112803;
    public const int WEDDING_RING_STAR = 1112806;
    public const int WEDDING_RING_GOLDEN = 1112807;
    public const int WEDDING_RING_SILVER = 1112809;

    public static bool isWeddingRing(int itemId)
    {
        return itemId == WEDDING_RING_MOONSTONE || itemId == WEDDING_RING_STAR ||
                itemId == WEDDING_RING_GOLDEN || itemId == WEDDING_RING_SILVER;
    }

    // Priority buff
    public const int ROSE_SCENT = 2022631;
    public const int FREESIA_SCENT = 2022632;
    public const int LAVENDER_SCENT = 2022633;

    // Cash shop
    public const int WHEEL_OF_FORTUNE = 5510000;
    public const int CASH_SHOP_SURPRISE = 5222000;
    public const int EXP_COUPON_2X_4H = 5211048;
    public const int DROP_COUPON_2X_4H = 5360042;
    public const int EXP_COUPON_3X_2H = 5211060;
    public const int QUICK_DELIVERY_TICKET = 5330000;
    public const int CHALKBOARD_1 = 5370000;
    public const int CHALKBOARD_2 = 5370001;
    public const int REMOTE_GACHAPON_TICKET = 5451000;
    public const int AP_RESET = 5050000;
    public const int NAME_CHANGE = 5400000;
    public const int WORLD_TRANSFER = 5401000;
    public const int MAPLE_LIFE_B = 5432000;
    public const int VICIOUS_HAMMER = 5570000;

    public const int NX_CARD_100 = 4031865;
    public const int NX_CARD_250 = 4031866;

    public static bool isNxCard(int itemId)
    {
        return itemId == NX_CARD_100 || itemId == NX_CARD_250;
    }

    public static bool isCashPackage(int itemId)
    {
        return itemId / 10000 == 910;
    }

    // Face expression
    private static int FACE_EXPRESSION_MIN = 5160000;
    private static int FACE_EXPRESSION_MAX = 5160014;

    public static bool isFaceExpression(int itemId)
    {
        return itemId >= FACE_EXPRESSION_MIN && itemId <= FACE_EXPRESSION_MAX;
    }

    // New Year card
    public static int NEW_YEARS_CARD = 2160101;
    public static int NEW_YEARS_CARD_SEND = 4300000;
    public static int NEW_YEARS_CARD_RECEIVED = 4301000;

    // Popular owl items
    private static int WORK_GLOVES = 1082002;
    private static int STEELY_THROWING_KNIVES = 2070005;
    private static int ILBI_THROWING_STARS = 2070006;
    private static int OWL_BALL_MASK = 1022047;
    private static int PINK_ADVENTURER_CAPE = 1102041;
    private static int CLAW_30_SCROLL = 2044705;
    private static int HELMET_60_ACC_SCROLL = 2040017;
    private static int MAPLE_SHIELD = 1092030;
    private static int GLOVES_ATT_60_SCROLL = 2040804;

    public static int[] getOwlItems()
    {
        return new int[]{WORK_GLOVES, STEELY_THROWING_KNIVES, ILBI_THROWING_STARS, OWL_BALL_MASK, PINK_ADVENTURER_CAPE,
                CLAW_30_SCROLL, WHITE_SCROLL, HELMET_60_ACC_SCROLL, MAPLE_SHIELD, GLOVES_ATT_60_SCROLL};
    }

    // Henesys PQ
    public const int GREEN_PRIMROSE_SEED = 4001095;
    public const int PURPLE_PRIMROSE_SEED = 4001096;
    public const int PINK_PRIMROSE_SEED = 4001097;
    public const int BROWN_PRIMROSE_SEED = 4001098;
    public const int YELLOW_PRIMROSE_SEED = 4001099;
    public const int BLUE_PRIMROSE_SEED = 4001100;
    public const int MOON_BUNNYS_RICE_CAKE = 4001101;

    // Catch mobs items
    public const int PHEROMONE_PERFUME = 2270000;
    public const int POUCH = 2270001;
    public const int GHOST_SACK = 4031830;
    public const int ARPQ_ELEMENT_ROCK = 2270002;
    public const int ARPQ_SPIRIT_JEWEL = 4031868;
    public const int MAGIC_CANE = 2270003;
    public const int TAMED_RUDOLPH = 4031887;
    public const int TRANSPARENT_MARBLE_1 = 2270005;
    public const int MONSTER_MARBLE_1 = 2109001;
    public const int TRANSPARENT_MARBLE_2 = 2270006;
    public const int MONSTER_MARBLE_2 = 2109002;
    public const int TRANSPARENT_MARBLE_3 = 2270007;
    public const int MONSTER_MARBLE_3 = 2109003;
    public const int EPQ_PURIFICATION_MARBLE = 2270004;
    public const int EPQ_MONSTER_MARBLE = 4001169;
    public const int FISH_NET = 2270008;
    public const int FISH_NET_WITH_A_CATCH = 2022323;

    // Mount
    public static int BATTLESHIP = 1932000;

    // Explorer mount
    public static int HOG = 1902000;
    private static int SILVER_MANE = 1902001;
    private static int RED_DRACO = 1902002;
    private static int EXPLORER_SADDLE = 1912000;

    public static bool isExplorerMount(int itemId)
    {
        return itemId >= HOG && itemId <= RED_DRACO || itemId == EXPLORER_SADDLE;
    }

    // Cygnus mount
    private static int MIMIANA = 1902005;
    private static int MIMIO = 1902006;
    private static int SHINJOU = 1902007;
    private static int CYGNUS_SADDLE = 1912005;

    public static bool isCygnusMount(int itemId)
    {
        return itemId >= MIMIANA && itemId <= SHINJOU || itemId == CYGNUS_SADDLE;
    }

    // Dev equips
    public static int GREEN_HEADBAND = 1002067;
    public static int TIMELESS_NIBLEHEIM = 1402046;
    public static int BLUE_KORBEN = 1082140;
    public static int MITHRIL_PLATINE_PANTS = 1060091;
    public static int BLUE_CARZEN_BOOTS = 1072154;
    public static int MITHRIL_PLATINE = 1040103;
}

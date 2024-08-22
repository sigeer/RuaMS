namespace constants.id;

public class MobId
{
    public const int ARPQ_BOMB = 9300166;
    public const int GIANT_CAKE = 9400606;
    public const int TRANSPARENT_ITEM = 9300216;

    public const int GREEN_MUSHROOM = 1110100;
    public const int DEJECTED_GREEN_MUSHROOM = 1110130;
    public const int GREEN_MUSHROOM_QUEST = 9101000;
    public const int ZOMBIE_MUSHROOM = 2230101;
    public const int ANNOYED_ZOMBIE_MUSHROOM = 2230131;
    public const int ZOMBIE_MUSHROOM_QUEST = 9101001;
    public const int GHOST_STUMP = 1140100;
    public const int SMIRKING_GHOST_STUMP = 1140130;
    public const int GHOST_STUMP_QUEST = 9101002;

    public const int PAPULATUS_CLOCK = 8500001;
    public const int HIGH_DARKSTAR = 8500003;
    public const int LOW_DARKSTAR = 8500004;

    public const int PIANUS_R = 8510000;
    public const int BLOODY_BOOM = 8510100;

    public const int PINK_BEAN = 8820001;

    public const int ZAKUM_1 = 8800000;
    public const int ZAKUM_2 = 8800001;
    public const int ZAKUM_3 = 8800002;
    public const int ZAKUM_ARM_1 = 8800003;
    public const int ZAKUM_ARM_2 = 8800004;
    public const int ZAKUM_ARM_3 = 8800005;
    public const int ZAKUM_ARM_4 = 8800006;
    public const int ZAKUM_ARM_5 = 8800007;
    public const int ZAKUM_ARM_6 = 8800008;
    public const int ZAKUM_ARM_7 = 8800009;
    public const int ZAKUM_ARM_8 = 8800010;

    public static bool isZakumArm(int mobId)
    {
        return mobId >= ZAKUM_ARM_1 && mobId <= ZAKUM_ARM_8;
    }

    public const int HORNTAIL_PREHEAD_LEFT = 8810000;
    public const int HORNTAIL_PREHEAD_RIGHT = 8810001;
    public const int HORNTAIL_HEAD_A = 8810002;
    public const int HORNTAIL_HEAD_B = 8810003;
    public const int HORNTAIL_HEAD_C = 8810004;
    public const int HORNTAIL_HAND_LEFT = 8810005;
    public const int HORNTAIL_HAND_RIGHT = 8810006;
    public const int HORNTAIL_WINGS = 8810007;
    public const int HORNTAIL_LEGS = 8810008;
    public const int HORNTAIL_TAIL = 8810009;
    public const int DEAD_HORNTAIL_MIN = 8810010;
    public const int DEAD_HORNTAIL_MAX = 8810017;
    public const int HORNTAIL = 8810018;
    public const int SUMMON_HORNTAIL = 8810026;

    public static bool isDeadHorntailPart(int mobId)
    {
        return mobId >= DEAD_HORNTAIL_MIN && mobId <= DEAD_HORNTAIL_MAX;
    }

    public const int SCARLION_STATUE = 9420546;
    public const int SCARLION = 9420547;
    public const int ANGRY_SCARLION = 9420548;
    public const int FURIOUS_SCARLION = 9420549;
    public const int TARGA_STATUE = 9420541;
    public const int TARGA = 9420542;
    public const int ANGRY_TARGA = 9420543;
    public const int FURIOUS_TARGA = 9420544;

    // Catch mobs
    public const int TAMABLE_HOG = 9300101;
    public const int GHOST = 9500197;
    public const int ARPQ_SCORPION = 9300157;
    public const int LOST_RUDOLPH = 9500320;
    public const int KING_SLIME_DOJO = 9300187;
    public const int FAUST_DOJO = 9300189;
    public const int MUSHMOM_DOJO = 9300191;
    public const int POISON_FLOWER = 9300175;
    public const int P_JUNIOR = 9500336;

    // Friendly mobs
    public const int WATCH_HOG = 9300102;
    public const int MOON_BUNNY = 9300061;
    public const int TYLUS = 9300093;
    public const int JULIET = 9300137;
    public const int ROMEO = 9300138;
    public const int DELLI = 9300162;
    public const int GIANT_SNOWMAN_LV1_EASY = 9400322;
    public const int GIANT_SNOWMAN_LV1_MEDIUM = 9400327;
    public const int GIANT_SNOWMAN_LV1_HARD = 9400332;
    public const int GIANT_SNOWMAN_LV5_EASY = 9400326;
    public const int GIANT_SNOWMAN_LV5_MEDIUM = 9400331;
    public const int GIANT_SNOWMAN_LV5_HARD = 9400336;

    // Dojo
    private static int DOJO_BOSS_MIN = 9300184;
    private static int DOJO_BOSS_MAX = 9300215;

    public static bool isDojoBoss(int mobId)
    {
        return mobId >= DOJO_BOSS_MIN && mobId <= DOJO_BOSS_MAX;
    }
}

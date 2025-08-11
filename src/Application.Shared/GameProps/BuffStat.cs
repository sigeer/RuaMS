using Application.Utility;

namespace Application.Shared.GameProps
{
    public class BuffStat : EnumClass
    {
        // public static readonly BuffStat SLOW = new BuffStat(0x1L);
        public static readonly BuffStat MORPH = new BuffStat(0x2L);
        public static readonly BuffStat RECOVERY = new BuffStat(0x4L);
        public static readonly BuffStat MAPLE_WARRIOR = new BuffStat(0x8L);
        public static readonly BuffStat STANCE = new BuffStat(0x10L);
        public static readonly BuffStat SHARP_EYES = new BuffStat(0x20L);
        public static readonly BuffStat MANA_REFLECTION = new BuffStat(0x40L);
        //public static readonly BuffStat ALWAYS_RIGHT = new BuffStat(0X80L);
        public static readonly BuffStat SHADOW_CLAW = new BuffStat(0x100L);
        public static readonly BuffStat INFINITY = new BuffStat(0x200L);
        public static readonly BuffStat HOLY_SHIELD = new BuffStat(0x400L);
        public static readonly BuffStat HAMSTRING = new BuffStat(0x800L);
        public static readonly BuffStat BLIND = new BuffStat(0x1000L);
        public static readonly BuffStat CONCENTRATE = new BuffStat(0x2000L);
        public static readonly BuffStat PUPPET = new BuffStat(0x4000L);
        public static readonly BuffStat ECHO_OF_HERO = new BuffStat(0x8000L);
        public static readonly BuffStat MESO_UP_BY_ITEM = new BuffStat(0x10000L);
        public static readonly BuffStat GHOST_MORPH = new BuffStat(0x20000L);
        public static readonly BuffStat AURA = new BuffStat(0x40000L);
        public static readonly BuffStat CONFUSE = new BuffStat(0x80000L);

        public static readonly BuffStat EXP_BUFF = new BuffStat(0x40000000L);

        // ------ COUPON feature ------
        public static readonly BuffStat COUPON_EXP1 = new BuffStat(0x100000L);
        public static readonly BuffStat COUPON_EXP2 = new BuffStat(0x200000L);
        public static readonly BuffStat COUPON_EXP3 = new BuffStat(0x400000L);
        public static readonly BuffStat COUPON_EXP4 = new BuffStat(0x400000L);
        public static readonly BuffStat COUPON_DRP1 = new BuffStat(0x800000L);
        public static readonly BuffStat COUPON_DRP2 = new BuffStat(0x1000000L);
        public static readonly BuffStat COUPON_DRP3 = new BuffStat(0x1000000L);

        // ------ monster card buffs, thanks to Arnah (Vertisy) ------
        public static readonly BuffStat ITEM_UP_BY_ITEM = new BuffStat(0x100000L);
        public static readonly BuffStat RESPECT_PIMMUNE = new BuffStat(0x200000L);
        public static readonly BuffStat RESPECT_MIMMUNE = new BuffStat(0x400000L);
        public static readonly BuffStat DEFENSE_ATT = new BuffStat(0x800000L);
        public static readonly BuffStat DEFENSE_STATE = new BuffStat(0x1000000L);

        public static readonly BuffStat HPREC = new BuffStat(0x2000000L);
        public static readonly BuffStat MPREC = new BuffStat(0x4000000L);
        public static readonly BuffStat BERSERK_FURY = new BuffStat(0x8000000L);
        public static readonly BuffStat DIVINE_BODY = new BuffStat(0x10000000L);
        public static readonly BuffStat SPARK = new BuffStat(0x20000000L);
        public static readonly BuffStat MAP_CHAIR = new BuffStat(0x40000000L);
        public static readonly BuffStat FINALATTACK = new BuffStat(0x80000000L);
        public static readonly BuffStat WATK = new BuffStat(0x100000000L);
        public static readonly BuffStat WDEF = new BuffStat(0x200000000L);
        public static readonly BuffStat MATK = new BuffStat(0x400000000L);
        public static readonly BuffStat MDEF = new BuffStat(0x800000000L);
        public static readonly BuffStat ACC = new BuffStat(0x1000000000L);
        public static readonly BuffStat AVOID = new BuffStat(0x2000000000L);
        public static readonly BuffStat HANDS = new BuffStat(0x4000000000L);
        public static readonly BuffStat SPEED = new BuffStat(0x8000000000L);
        public static readonly BuffStat JUMP = new BuffStat(0x10000000000L);
        public static readonly BuffStat MAGIC_GUARD = new BuffStat(0x20000000000L);
        public static readonly BuffStat DARKSIGHT = new BuffStat(0x40000000000L);
        public static readonly BuffStat BOOSTER = new BuffStat(0x80000000000L);
        public static readonly BuffStat POWERGUARD = new BuffStat(0x100000000000L);
        public static readonly BuffStat HYPERBODYHP = new BuffStat(0x200000000000L);
        public static readonly BuffStat HYPERBODYMP = new BuffStat(0x400000000000L);
        public static readonly BuffStat INVINCIBLE = new BuffStat(0x800000000000L);
        public static readonly BuffStat SOULARROW = new BuffStat(0x1000000000000L);
        public static readonly BuffStat STUN = new BuffStat(0x2000000000000L);
        public static readonly BuffStat POISON = new BuffStat(0x4000000000000L);
        public static readonly BuffStat SEAL = new BuffStat(0x8000000000000L);
        public static readonly BuffStat DARKNESS = new BuffStat(0x10000000000000L);
        public static readonly BuffStat COMBO = new BuffStat(0x20000000000000L);
        public static readonly BuffStat SUMMON = new BuffStat(0x20000000000000L);
        public static readonly BuffStat WK_CHARGE = new BuffStat(0x40000000000000L);
        public static readonly BuffStat DRAGONBLOOD = new BuffStat(0x80000000000000L);
        public static readonly BuffStat HOLY_SYMBOL = new BuffStat(0x100000000000000L);
        public static readonly BuffStat MESOUP = new BuffStat(0x200000000000000L);
        public static readonly BuffStat SHADOWPARTNER = new BuffStat(0x400000000000000L);
        public static readonly BuffStat PICKPOCKET = new BuffStat(0x800000000000000L);
        public static readonly BuffStat MESOGUARD = new BuffStat(0x1000000000000000L);
        public static readonly BuffStat EXP_INCREASE = new BuffStat(0x2000000000000000L);
        public static readonly BuffStat WEAKEN = new BuffStat(0x4000000000000000L);
        public static readonly BuffStat MAP_PROTECTION = new BuffStat(long.MinValue);

        //all incorrect buffstats
        public static readonly BuffStat SLOW = new BuffStat(0x200000000L, true);
        public static readonly BuffStat ELEMENTAL_RESET = new BuffStat(0x200000000L, true);
        public static readonly BuffStat MAGIC_SHIELD = new BuffStat(0x400000000L, true);
        public static readonly BuffStat MAGIC_RESISTANCE = new BuffStat(0x800000000L, true);
        // needs Soul Stone
        //end incorrect buffstats

        public static readonly BuffStat WIND_WALK = new BuffStat(0x400000000L, true);
        public static readonly BuffStat ARAN_COMBO = new BuffStat(0x1000000000L, true);
        public static readonly BuffStat COMBO_DRAIN = new BuffStat(0x2000000000L, true);
        public static readonly BuffStat COMBO_BARRIER = new BuffStat(0x4000000000L, true);
        public static readonly BuffStat BODY_PRESSURE = new BuffStat(0x8000000000L, true);
        public static readonly BuffStat SMART_KNOCKBACK = new BuffStat(0x10000000000L, true);
        public static readonly BuffStat BERSERK = new BuffStat(0x20000000000L, true);
        public static readonly BuffStat ENERGY_CHARGE = new BuffStat(0x4000000000000L, true);
        public static readonly BuffStat DASH2 = new BuffStat(0x8000000000000L, true); // correct (speed)
        public static readonly BuffStat DASH = new BuffStat(0x10000000000000L, true); // correct (jump)
        public static readonly BuffStat MONSTER_RIDING = new BuffStat(0x20000000000000L, true);
        public static readonly BuffStat SPEED_INFUSION = new BuffStat(0x40000000000000L, true);
        public static readonly BuffStat HOMING_BEACON = new BuffStat(0x80000000000000L, true);

        private long i;
        private bool isFirst;

        BuffStat(long i, bool isFirst = false)
        {
            this.i = i;
            this.isFirst = isFirst;
        }

        public long getValue()
        {
            return i;
        }

        public bool IsFirst => isFirst;


        public override string ToString()
        {
            return name();
        }

        public int CompareTo(BuffStat? other)
        {
            if (other == null)
                return 1;
            return i.CompareTo(other.i);
        }
    }
}

using Application.Shared.GameProps;
using Application.Utility;

namespace ServiceTest.Infrastructure
{
    public class TempDe
    {
        [Test]
        public void P1()
        {
            var n = Enum.GetValues<TemporaryStatType>();
            List<PosValuePair> ns = [];
            foreach (var item in n)
            {
                ns.Add(
                    new(
                        item.ToString(),
                        3 - ((int)item / 32),
                        (uint)(((long)1 << ((int)item % 32)) & 0xFFFFFFFF),
                        false
                    )
                );
            }

            var o = EnumClassCache<BuffStat>.GetValues();
            var d = EnumClassCache<Disease>.GetValues();
            List<PosValuePair> os = [];
            foreach (var item in o)
            {
                var high = (uint)(item.getValue() >> 32);
                var low = (uint)(item.getValue() & 0xFFFFFFFF);

                int pos = low != 0 ? 2 : 3;
                if (item.IsFirst)
                {
                    pos -= 2;
                }
                var value = low > 0 ? low : high;
                os.Add(new PosValuePair(item.name(), pos, value, false));
            }
            foreach (var item in d)
            {
                var high = (uint)(item.getValue() >> 32);
                var low = (uint)(item.getValue() & 0xFFFFFFFF);

                int pos = low != 0 ? 2 : 3;
                if (item.isFirst())
                {
                    pos -= 2;
                }
                var value = low > 0 ? low : high;
                os.Add(new PosValuePair(item.name(), pos, value, true));
            }

            Console.WriteLine("New================>");
            foreach (var item in ns.OrderBy(x => x.Position).ThenBy(x => x.Value))
            {
                Console.WriteLine($"{item.Name}: Pos: {item.Position}, Value: {(uint)item.Value}");
            }

            Console.WriteLine("Old================>");
            foreach (var item in os.OrderBy(x => x.Position).ThenBy(x => x.Value))
            {
                var s = $"{item.Name}: Pos: {item.Position}, Value: {(uint)item.Value}";
                if (item.IsDisease)
                {
                    s += "  (From Disease)";
                }
                Console.WriteLine(s);
            }

            Assert.Pass();
        }
    }

    public record PosValuePair(string Name, int Position, uint Value, bool IsDisease);

    public enum TemporaryStatType
    {
        WATK,
        WDEF,
        MATK,
        MDEF,
        ACC,
        AVOID,
        HANDS,
        SPEED,
        JUMP,
        MAGIC_GUARD,
        DARKSIGHT,
        BOOSTER,
        POWERGUARD,
        HYPERBODYHP,
        HYPERBODYMP,
        INVINCIBLE,
        SOULARROW,
        STUN,
        POISON,
        SEAL,
        DARKNESS,
        COMBO,
        WK_CHARGE,
        DRAGONBLOOD,
        HOLY_SYMBOL,
        MESOUP,
        SHADOWPARTNER,
        PICKPOCKET,
        MESOGUARD,
        THAW,
        WEAKEN,
        CURSE,

        SLOW,
        MORPH,
        RECOVERY,
        MAPLE_WARRIOR,
        STANCE,
        SHARP_EYES,
        MANA_REFLECTION,
        SEDUCE,
        SPIRITJAVELIN,
        INFINITY,
        HOLY_SHIELD,
        HAMSTRING,
        BLIND,
        CONCENTRATE,
        BANMAP,
        ECHO_OF_HERO,
        MESO_UP_BY_ITEM,
        GHOST_MORPH,
        AURA,
        CONFUSE,
        ITEM_UP_BY_ITEM,
        RESPECT_PIMMUNE,
        RESPECT_MIMMUNE,
        DEFENSE_ATT,
        DEFENSE_STATE,
        HPREC,
        MPREC,
        BERSERK_FURY,
        DIVINE_BODY,
        SPARK,
        DOJANGSHIELD,
        SOULMASTERFINAL,

        WINDBREAKERFINAL,
        ELEMENTALRESET,
        WIND_WALK,
        EVENTRATE,
        ARAN_COMBO,
        COMBO_DRAIN,
        COMBO_BARRIER,
        BODY_PRESSURE,
        SMART_KNOCKBACK,
        BERSERK,
        EXPBUFFRATE,
        StopPotion,
        StopMotion,
        FEAR,
        EVANSLOW,
        MAGICSHIELD,
        MAGICRESISTANCE,
        SOULSTONE,
        ENERGY_CHARGE,
        DASH2,
        DASH,
        MONSTER_RIDING,
        ENRAGE,
        GuidedBullet,
        NOTDAMAGED,
        ZOMBIFY,
    }

    //New================>
    //WINDBREAKERFINAL: Pos: 1, Value: 1
    //ELEMENTALRESET: Pos: 1, Value: 2
    //WIND_WALK: Pos: 1, Value: 4
    //EVENTRATE: Pos: 1, Value: 8
    //ARAN_COMBO: Pos: 1, Value: 16
    //COMBO_DRAIN: Pos: 1, Value: 32
    //COMBO_BARRIER: Pos: 1, Value: 64
    //BODY_PRESSURE: Pos: 1, Value: 128
    //SMART_KNOCKBACK: Pos: 1, Value: 256
    //BERSERK: Pos: 1, Value: 512
    //EXPBUFFRATE: Pos: 1, Value: 1024
    //StopPotion: Pos: 1, Value: 2048
    //StopMotion: Pos: 1, Value: 4096
    //FEAR: Pos: 1, Value: 8192
    //EVANSLOW: Pos: 1, Value: 16384
    //MAGICSHIELD: Pos: 1, Value: 32768
    //MAGICRESISTANCE: Pos: 1, Value: 65536
    //SOULSTONE: Pos: 1, Value: 131072
    //ENERGY_CHARGE: Pos: 1, Value: 262144
    //DASH2: Pos: 1, Value: 524288
    //DASH: Pos: 1, Value: 1048576
    //MONSTER_RIDING: Pos: 1, Value: 2097152
    //ENRAGE: Pos: 1, Value: 4194304
    //GuidedBullet: Pos: 1, Value: 8388608
    //NOTDAMAGED: Pos: 1, Value: 16777216
    //ZOMBIFY: Pos: 1, Value: 33554432
    //SLOW: Pos: 2, Value: 1
    //MORPH: Pos: 2, Value: 2
    //RECOVERY: Pos: 2, Value: 4
    //MAPLE_WARRIOR: Pos: 2, Value: 8
    //STANCE: Pos: 2, Value: 16
    //SHARP_EYES: Pos: 2, Value: 32
    //MANA_REFLECTION: Pos: 2, Value: 64
    //SEDUCE: Pos: 2, Value: 128
    //SPIRITJAVELIN: Pos: 2, Value: 256
    //INFINITY: Pos: 2, Value: 512
    //HOLY_SHIELD: Pos: 2, Value: 1024
    //HAMSTRING: Pos: 2, Value: 2048
    //BLIND: Pos: 2, Value: 4096
    //CONCENTRATE: Pos: 2, Value: 8192
    //BANMAP: Pos: 2, Value: 16384
    //ECHO_OF_HERO: Pos: 2, Value: 32768
    //MESO_UP_BY_ITEM: Pos: 2, Value: 65536
    //GHOST_MORPH: Pos: 2, Value: 131072
    //AURA: Pos: 2, Value: 262144
    //CONFUSE: Pos: 2, Value: 524288
    //ITEM_UP_BY_ITEM: Pos: 2, Value: 1048576
    //RESPECT_PIMMUNE: Pos: 2, Value: 2097152
    //RESPECT_MIMMUNE: Pos: 2, Value: 4194304
    //DEFENSE_ATT: Pos: 2, Value: 8388608
    //DEFENSE_STATE: Pos: 2, Value: 16777216
    //HPREC: Pos: 2, Value: 33554432
    //MPREC: Pos: 2, Value: 67108864
    //BERSERK_FURY: Pos: 2, Value: 134217728
    //DIVINE_BODY: Pos: 2, Value: 268435456
    //SPARK: Pos: 2, Value: 536870912
    //DOJANGSHIELD: Pos: 2, Value: 1073741824
    //SOULMASTERFINAL: Pos: 2, Value: 2147483648
    //WATK: Pos: 3, Value: 1
    //WDEF: Pos: 3, Value: 2
    //MATK: Pos: 3, Value: 4
    //MDEF: Pos: 3, Value: 8
    //ACC: Pos: 3, Value: 16
    //AVOID: Pos: 3, Value: 32
    //HANDS: Pos: 3, Value: 64
    //SPEED: Pos: 3, Value: 128
    //JUMP: Pos: 3, Value: 256
    //MAGIC_GUARD: Pos: 3, Value: 512
    //DARKSIGHT: Pos: 3, Value: 1024
    //BOOSTER: Pos: 3, Value: 2048
    //POWERGUARD: Pos: 3, Value: 4096
    //HYPERBODYHP: Pos: 3, Value: 8192
    //HYPERBODYMP: Pos: 3, Value: 16384
    //INVINCIBLE: Pos: 3, Value: 32768
    //SOULARROW: Pos: 3, Value: 65536
    //STUN: Pos: 3, Value: 131072
    //POISON: Pos: 3, Value: 262144
    //SEAL: Pos: 3, Value: 524288
    //DARKNESS: Pos: 3, Value: 1048576
    //COMBO: Pos: 3, Value: 2097152
    //WK_CHARGE: Pos: 3, Value: 4194304
    //DRAGONBLOOD: Pos: 3, Value: 8388608
    //HOLY_SYMBOL: Pos: 3, Value: 16777216
    //MESOUP: Pos: 3, Value: 33554432
    //SHADOWPARTNER: Pos: 3, Value: 67108864
    //PICKPOCKET: Pos: 3, Value: 134217728
    //MESOGUARD: Pos: 3, Value: 268435456
    //THAW: Pos: 3, Value: 536870912
    //WEAKEN: Pos: 3, Value: 1073741824
    //CURSE: Pos: 3, Value: 2147483648

    //Old================>
    //SLOW: Pos: 1, Value: 2
    //ELEMENTAL_RESET: Pos: 1, Value: 2
    //MAGIC_SHIELD: Pos: 1, Value: 4
    //WIND_WALK: Pos: 1, Value: 4
    //MAGIC_RESISTANCE: Pos: 1, Value: 8
    //ARAN_COMBO: Pos: 1, Value: 16
    //COMBO_DRAIN: Pos: 1, Value: 32
    //COMBO_BARRIER: Pos: 1, Value: 64
    //BODY_PRESSURE: Pos: 1, Value: 128
    //SMART_KNOCKBACK: Pos: 1, Value: 256
    //BERSERK: Pos: 1, Value: 512
    //EXP_BUFF: Pos: 1, Value: 1024
    //StopPotion: Pos: 1, Value: 2048  (From Disease)
    //StopMotion: Pos: 1, Value: 4096  (From Disease)
    //Blind: Pos: 1, Value: 8192  (From Disease)
    //ENERGY_CHARGE: Pos: 1, Value: 262144
    //DASH2: Pos: 1, Value: 524288
    //DASH: Pos: 1, Value: 1048576
    //MONSTER_RIDING: Pos: 1, Value: 2097152
    //SPEED_INFUSION: Pos: 1, Value: 4194304
    //HOMING_BEACON: Pos: 1, Value: 8388608
    //SLOW: Pos: 2, Value: 1  (From Disease)
    //MORPH: Pos: 2, Value: 2
    //RECOVERY: Pos: 2, Value: 4
    //MAPLE_WARRIOR: Pos: 2, Value: 8
    //STANCE: Pos: 2, Value: 16
    //SHARP_EYES: Pos: 2, Value: 32
    //MANA_REFLECTION: Pos: 2, Value: 64
    //SEDUCE: Pos: 2, Value: 128  (From Disease)
    //SHADOW_CLAW: Pos: 2, Value: 256
    //FISHABLE: Pos: 2, Value: 256  (From Disease)
    //INFINITY: Pos: 2, Value: 512
    //HOLY_SHIELD: Pos: 2, Value: 1024
    //HAMSTRING: Pos: 2, Value: 2048
    //BLIND: Pos: 2, Value: 4096
    //CONCENTRATE: Pos: 2, Value: 8192
    //PUPPET: Pos: 2, Value: 16384
    //ZOMBIFY: Pos: 2, Value: 16384  (From Disease)
    //ECHO_OF_HERO: Pos: 2, Value: 32768
    //MESO_UP_BY_ITEM: Pos: 2, Value: 65536
    //GHOST_MORPH: Pos: 2, Value: 131072
    //AURA: Pos: 2, Value: 262144
    //CONFUSE: Pos: 2, Value: 524288
    //CONFUSE: Pos: 2, Value: 524288  (From Disease)
    //COUPON_EXP1: Pos: 2, Value: 1048576
    //ITEM_UP_BY_ITEM: Pos: 2, Value: 1048576
    //COUPON_EXP2: Pos: 2, Value: 2097152
    //RESPECT_PIMMUNE: Pos: 2, Value: 2097152
    //COUPON_EXP3: Pos: 2, Value: 4194304
    //COUPON_EXP4: Pos: 2, Value: 4194304
    //RESPECT_MIMMUNE: Pos: 2, Value: 4194304
    //COUPON_DRP1: Pos: 2, Value: 8388608
    //DEFENSE_ATT: Pos: 2, Value: 8388608
    //COUPON_DRP2: Pos: 2, Value: 16777216
    //COUPON_DRP3: Pos: 2, Value: 16777216
    //DEFENSE_STATE: Pos: 2, Value: 16777216
    //HPREC: Pos: 2, Value: 33554432
    //MPREC: Pos: 2, Value: 67108864
    //BERSERK_FURY: Pos: 2, Value: 134217728
    //DIVINE_BODY: Pos: 2, Value: 268435456
    //SPARK: Pos: 2, Value: 536870912
    //MAP_CHAIR: Pos: 2, Value: 1073741824
    //FINALATTACK: Pos: 2, Value: 2147483648
    //NULL: Pos: 3, Value: 0  (From Disease)
    //WATK: Pos: 3, Value: 1
    //WDEF: Pos: 3, Value: 2
    //MATK: Pos: 3, Value: 4
    //MDEF: Pos: 3, Value: 8
    //ACC: Pos: 3, Value: 16
    //AVOID: Pos: 3, Value: 32
    //HANDS: Pos: 3, Value: 64
    //SPEED: Pos: 3, Value: 128
    //JUMP: Pos: 3, Value: 256
    //MAGIC_GUARD: Pos: 3, Value: 512
    //DARKSIGHT: Pos: 3, Value: 1024
    //BOOSTER: Pos: 3, Value: 2048
    //POWERGUARD: Pos: 3, Value: 4096
    //HYPERBODYHP: Pos: 3, Value: 8192
    //HYPERBODYMP: Pos: 3, Value: 16384
    //INVINCIBLE: Pos: 3, Value: 32768
    //SOULARROW: Pos: 3, Value: 65536
    //STUN: Pos: 3, Value: 131072
    //STUN: Pos: 3, Value: 131072  (From Disease)
    //POISON: Pos: 3, Value: 262144
    //POISON: Pos: 3, Value: 262144  (From Disease)
    //SEAL: Pos: 3, Value: 524288
    //SEAL: Pos: 3, Value: 524288  (From Disease)
    //DARKNESS: Pos: 3, Value: 1048576
    //DARKNESS: Pos: 3, Value: 1048576  (From Disease)
    //COMBO: Pos: 3, Value: 2097152
    //SUMMON: Pos: 3, Value: 2097152
    //WK_CHARGE: Pos: 3, Value: 4194304
    //DRAGONBLOOD: Pos: 3, Value: 8388608
    //HOLY_SYMBOL: Pos: 3, Value: 16777216
    //MESOUP: Pos: 3, Value: 33554432
    //SHADOWPARTNER: Pos: 3, Value: 67108864
    //PICKPOCKET: Pos: 3, Value: 134217728
    //MESOGUARD: Pos: 3, Value: 268435456
    //EXP_INCREASE: Pos: 3, Value: 536870912
    //WEAKEN: Pos: 3, Value: 1073741824
    //WEAKEN: Pos: 3, Value: 1073741824  (From Disease)
    //MAP_PROTECTION: Pos: 3, Value: 2147483648
    //CURSE: Pos: 3, Value: 2147483648  (From Disease)

}

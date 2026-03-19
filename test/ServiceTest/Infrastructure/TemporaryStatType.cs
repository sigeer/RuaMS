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
                ns.Add(new(item.ToString(), 3 - ((int)item / 32), (uint)(((long)1 << ((int)item % 32)) & 0xFFFFFFFF), false));
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
        ASSISTCHARGE,
        ENRAGE,
        SUDDENDEATH,
        NOTDAMAGED,
        ZOMBIFY
    }
}

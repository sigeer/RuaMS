using Application.Utility;
using Application.Utility.Exceptions;
using Serilog;

namespace Application.Shared.GameProps
{
    public class Disease : EnumClass
    {
        // 定义静态的常量实例
        public static readonly Disease NULL = new(0x0);
        public static readonly Disease SLOW = new(0x1, MobSkillType.SLOW);
        public static readonly Disease SEDUCE = new(0x80, MobSkillType.SEDUCE);
        public static readonly Disease FISHABLE = new(0x100);
        public static readonly Disease ZOMBIFY = new(0x4000);
        public static readonly Disease CONFUSE = new(0x80000, MobSkillType.REVERSE_INPUT);
        public static readonly Disease STUN = new(0x2000000000000L, MobSkillType.STUN);
        public static readonly Disease POISON = new(0x4000000000000L, MobSkillType.POISON);
        public static readonly Disease SEAL = new(0x8000000000000L, MobSkillType.SEAL);
        public static readonly Disease DARKNESS = new(0x10000000000000L, MobSkillType.DARKNESS);
        public static readonly Disease WEAKEN = new(0x4000000000000000L, MobSkillType.WEAKNESS);
        public static readonly Disease CURSE = new(0x8000000000000000L, MobSkillType.CURSE);

        private ulong i;
        private MobSkillType? mobSkillType;

        Disease(ulong i) : this(i, null)
        {

        }

        Disease(ulong i, MobSkillType? skill)
        {
            this.i = i;
            this.mobSkillType = skill;
        }

        public ulong getValue()
        {
            return i;
        }

        public bool isFirst()
        {
            return false;
        }

        public MobSkillType? getMobSkillType()
        {
            return mobSkillType;
        }

        public static Disease ordinal(int ord)
        {
            try
            {
                return EnumClassCache<Disease>.Values[ord];
            }
            catch (IndexOutOfRangeException e)
            {
                Log.Logger.Error(e.ToString());
                return NULL;
            }
        }


        public static Disease? getBySkill(MobSkillType? skill)
        {
            return EnumClassCache<Disease>.Values.FirstOrDefault(x => x.mobSkillType == skill);
        }

        public static Disease GetBySkillTrust(MobSkillType? skill)
        {
            return getBySkill(skill) ?? throw new BusinessResException($"getBySkill({skill})");
        }

    }
}

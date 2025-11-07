using Application.Utility;
using Application.Utility.Exceptions;
using Serilog;

namespace Application.Shared.GameProps
{
    public class Disease : EnumClass
    {
        // 定义静态的常量实例
        public static readonly Disease NULL = new(0x0);
        /// <summary>
        /// 减速
        /// </summary>
        public static readonly Disease SLOW = new(0x1, MobSkillType.SLOW);
        /// <summary>
        /// 迷惑
        /// </summary>
        public static readonly Disease SEDUCE = new(0x80, MobSkillType.SEDUCE);
        public static readonly Disease FISHABLE = new(0x100);
        public static readonly Disease ZOMBIFY = new(0x4000, MobSkillType.UNDEAD);
        /// <summary>
        /// 混乱
        /// </summary>
        public static readonly Disease CONFUSE = new(0x80000, MobSkillType.REVERSE_INPUT);
        /// <summary>
        /// 眩晕
        /// </summary>
        public static readonly Disease STUN = new(0x2000000000000L, MobSkillType.STUN);
        /// <summary>
        /// 中毒
        /// </summary>
        public static readonly Disease POISON = new(0x4000000000000L, MobSkillType.POISON);
        /// <summary>
        /// 封印
        /// </summary>
        public static readonly Disease SEAL = new(0x8_0000_0000_0000L, MobSkillType.SEAL);
        /// <summary>
        /// 暗黑
        /// </summary>
        public static readonly Disease DARKNESS = new(0x10000000000000L, MobSkillType.DARKNESS);
        /// <summary>
        /// 虚弱
        /// </summary>
        public static readonly Disease WEAKEN = new(0x4000000000000000L, MobSkillType.WEAKNESS);
        /// <summary>
        /// 诅咒
        /// </summary>
        public static readonly Disease CURSE = new(0x8000_0000_0000_0000L, MobSkillType.CURSE);
        /// <summary>
        /// 无法喝药
        /// </summary>
        public static readonly Disease StopPotion = new Disease(0x800_0000_0000, MobSkillType.STOP_POTION, true);
        /// <summary>
        /// 无法移动
        /// </summary>
        public static readonly Disease StopMotion = new Disease(0x1000_0000_0000, MobSkillType.STOP_MOTION, true);
        /// <summary>
        /// 致盲
        /// </summary>
        public static readonly Disease Blind = new Disease(0x2000_0000_0000, MobSkillType.FEAR, true);

        private ulong i;
        private MobSkillType mobSkillType;
        bool _isFirst;

        Disease(ulong i) : this(i, 0)
        {

        }

        Disease(ulong i, MobSkillType skill) : this(i, 0, false)
        {

        }

        Disease(ulong i, MobSkillType skill, bool priority)
        {
            this.i = i;
            this.mobSkillType = skill;
            this._isFirst = priority;
        }

        public ulong getValue()
        {
            return i;
        }

        public bool isFirst()
        {
            return _isFirst;
        }

        public MobSkillType getMobSkillType()
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

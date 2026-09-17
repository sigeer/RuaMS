using Application.Utility.Exceptions;

namespace Application.Shared.GameProps
{
    /// <summary>
    /// 疾病（debuff）相关的元数据。
    ///
    /// <para>
    /// 疾病本身已经并入 <see cref="BuffStat"/>——客户端本来就只用一张 TemporaryStat 表
    /// （位号就是 <see cref="BuffStat"/> 的值），服务端再分成两个类型只会多一层换算。
    /// 这里只保留 <see cref="BuffStat"/> 表达不了的信息：触发它的 <see cref="MobSkillType"/>。
    /// </para>
    /// </summary>
    public static class DiseaseInfo
    {
        /// <summary>疾病 → 触发它的 mob skill 类型。</summary>
        private static readonly Dictionary<BuffStat, MobSkillType> _mobSkillTypeByDisease = new()
        {
            [BuffStat.SLOW] = MobSkillType.SLOW,
            [BuffStat.SEDUCE] = MobSkillType.SEDUCE,
            [BuffStat.ZOMBIFY] = MobSkillType.UNDEAD,
            [BuffStat.CONFUSE] = MobSkillType.REVERSE_INPUT,
            [BuffStat.STUN] = MobSkillType.STUN,
            [BuffStat.POISON] = MobSkillType.POISON,
            [BuffStat.SEAL] = MobSkillType.SEAL,
            [BuffStat.DARKNESS] = MobSkillType.DARKNESS,
            [BuffStat.WEAKEN] = MobSkillType.WEAKNESS,
            [BuffStat.CURSE] = MobSkillType.CURSE,
            [BuffStat.StopPotion] = MobSkillType.STOP_POTION,
            [BuffStat.StopMotion] = MobSkillType.STOP_MOTION,
            [BuffStat.FEAR] = MobSkillType.FEAR,
        };

        private static readonly Dictionary<MobSkillType, BuffStat> _diseaseByMobSkill
            = _mobSkillTypeByDisease.ToDictionary(x => x.Value, x => x.Key);

        /// <summary>所有有 mob skill 来源的疾病（顺序不保证）。</summary>
        public static IEnumerable<BuffStat> Diseases => _mobSkillTypeByDisease.Keys;

        public static bool IsDisease(BuffStat stat) => _mobSkillTypeByDisease.ContainsKey(stat);

        public static MobSkillType? GetMobSkillType(BuffStat stat)
            => _mobSkillTypeByDisease.TryGetValue(stat, out MobSkillType skillType) ? skillType : null;

        public static BuffStat? GetBySkill(MobSkillType? skill)
            => skill != null && _diseaseByMobSkill.TryGetValue(skill.Value, out BuffStat stat) ? stat : null;

        public static BuffStat GetBySkillTrust(MobSkillType? skill)
            => GetBySkill(skill) ?? throw new BusinessResException($"GetBySkill({skill})");

        /// <summary>怪物卡片「状态抗性」用的缩写。</summary>
        public static BuffStat? GetByAb(string? val)
        {
            switch (val)
            {
                case "C":
                    return BuffStat.CURSE;
                case "F":
                    return BuffStat.STUN;
                case "D":
                    return BuffStat.DARKNESS;
                case "W":
                    return BuffStat.WEAKEN;
                case "S":
                    return BuffStat.SEAL;
                default:
                    return null;
            }
        }
    }
}

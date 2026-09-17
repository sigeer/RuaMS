using Application.Shared.GameProps;

namespace Application.Shared.Battle.Skills
{
    public record MobSkillId(MobSkillType type, int level)
    {
        /// <summary>
        /// 客户端包里的“来源”字段（buff 来源 / 疾病来源）：低 16 位技能类型，高 16 位技能等级。
        /// 对应 <c>sub_788156</c>（DecodeForRemote）疾病分支读到的那个 DWORD。
        /// </summary>
        public int getEncodedId()
        {
            return (type.getId() & 0xFFFF) | ((level & 0xFFFF) << 16);
        }
    }
}

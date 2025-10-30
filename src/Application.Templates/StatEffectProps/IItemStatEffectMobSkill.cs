using Application.Templates.Item.Consume;

namespace Application.Templates.StatEffectProps
{
    /// <summary>
    /// 使用后触发mobskill
    /// </summary>
    public interface IItemStatEffectMobSkill : IItemStatEffectProp
    {
        public MobSkillEffectData? MobSkill { get; }
    }
}

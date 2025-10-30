namespace Application.Templates.StatEffectProps
{
    /// <summary>
    /// 使用后触发mcskill
    /// </summary>
    public interface IItemStatEffectMC : IItemStatEffectProp
    {
        public int CP { get; }
        public int CPSkill { get; }
    }
}

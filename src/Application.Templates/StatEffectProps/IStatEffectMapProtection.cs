namespace Application.Templates.StatEffectProps
{
    public interface IStatEffectMapProtection: IStatEffectProp
    {
        /// <summary>
        /// 10. 寒冷保护，-6. 水下保护
        /// </summary>
        public int Thaw { get; }
    }
}

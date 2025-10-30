using Application.Templates.StatEffectProps;

namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 效果道具
    /// </summary>
    public abstract class EffectCashItemTemplate : ItemTemplateBase, IItemStatEffectProp
    {
        protected EffectCashItemTemplate(int templateId) : base(templateId)
        {
        }

        [GenerateIgnoreProperty]
        public int SourceId => TemplateId;
    }
}

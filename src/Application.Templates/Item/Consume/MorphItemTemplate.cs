using Application.Templates.Item.Consume;

namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 530, 221
    /// </summary>
    [GenerateTag]
    public sealed class MorphItemTemplate : ItemTemplateBase, IMorphPotionItem
    {
        [WZPath("spec/hp")]
        public int HP { get; set; }

        [WZPath("spec/morph")]
        public int Morph { get; set; }
        [WZPath("spec/morphRandom/-")]
        public MorphRandomData[] MorphRandom { get; set; }

        public MorphItemTemplate(int templateId)
            : base(templateId) 
        {
            MorphRandom = Array.Empty<MorphRandomData>();
        }
    }
}
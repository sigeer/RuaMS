namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 530
    /// </summary>
    [GenerateTag]
    public sealed class MorphItemTemplate : CashItemTemplate
    {
        [WZPath("spec/hp")]
        public int HP { get; set; }

        [WZPath("spec/morph")]
        public int Morph { get; set; }

        public MorphItemTemplate(int templateId)
            : base(templateId) { }
    }
}
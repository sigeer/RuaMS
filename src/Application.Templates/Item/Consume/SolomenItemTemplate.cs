namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 237
    /// </summary>
    public sealed class SolomenItemTemplate : ConsumeItemTemplate
    {
        public SolomenItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/maxLevel")]
        public int MaxLevel { get; set; }
        [WZPath("spec/exp")]
        public int Exp { get; set; }
    }
}

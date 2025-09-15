namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public sealed class TownScrollItemTemplate : ConsumeItemTemplate, IEffectItem
    {
        public TownScrollItemTemplate(int templateId) : base(templateId)
        {
        }
        [WZPath("spec/moveTo")]
        public int MoveTo { get; set; } = -1;
        [WZPath("spec/ignoreContinent")]
        public bool IgnoreContinent { get; set; }
    }
}

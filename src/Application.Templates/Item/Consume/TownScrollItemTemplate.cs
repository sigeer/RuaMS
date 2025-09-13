namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public sealed class TownScrollItemTemplate : ConsumeItemTemplate
    {
        public TownScrollItemTemplate(int templateId) : base(templateId)
        {
        }
        [WZPath("spec/moveTo")]
        public int MoveTo { get; set; }
        [WZPath("spec/ignoreContinent")]
        public bool IgnoreContinent { get; set; }
    }
}

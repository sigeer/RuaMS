namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 236
    /// </summary>
    [GenerateTag]
    public class GhostItemTemplate : ConsumeItemTemplate, IEffectItem
    {
        public GhostItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("spec/ghost")]
        public int Ghost { get; set; }
    }
}

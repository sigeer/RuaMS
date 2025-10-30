namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 512
    /// </summary>
    [GenerateTag]
    public sealed class MapBuffItemTemplate : ItemTemplateBase
    {
        public MapBuffItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/stateChangeItem")]
        public int StateChangeItem { get; set; }
    }
}

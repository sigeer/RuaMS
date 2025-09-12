namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 512
    /// </summary>
    [GenerateTag]
    public class MapBuffItemTemplate : CashItemTemplate
    {
        public MapBuffItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/stateChangeItem")]
        public int StateChangeItem { get; set; }
    }
}

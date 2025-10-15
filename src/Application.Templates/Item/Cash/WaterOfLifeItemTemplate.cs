namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 5180000
    /// </summary>
    [GenerateTag]
    public sealed class WaterOfLifeItemTemplate : CashItemTemplate
    {
        public WaterOfLifeItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/life")]
        public int Life { get; set; }
    }
}

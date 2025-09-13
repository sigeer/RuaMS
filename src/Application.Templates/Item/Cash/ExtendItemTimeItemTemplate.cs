namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 550
    /// </summary>
    [GenerateTag]
    public class ExtendItemTimeItemTemplate : CashItemTemplate
    {
        public ExtendItemTimeItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/addTime")]
        public int AddTime { get; set; }

        [WZPath("info/maxDays")]
        public int MaxDays { get; set; }
    }
}

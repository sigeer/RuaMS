namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 521, 536
    /// </summary>
    [GenerateTag]
    public class CouponItemTemplate : CashItemTemplate
    {
        public CouponItemTemplate(int templateId) : base(templateId)
        {
            TimeRange = Array.Empty<string>();
        }

        [WZPath("info/rate")]
        public float Rate { get; set; }

        [WZPath("info/time/-")]
        public string[] TimeRange { get; set; }
    }
}

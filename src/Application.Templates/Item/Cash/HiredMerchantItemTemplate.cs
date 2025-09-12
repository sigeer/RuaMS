namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 503
    /// </summary>
    [GenerateTag]
    public class HiredMerchantItemTemplate : CashItemTemplate
    {
        public HiredMerchantItemTemplate(int templateId) : base(templateId)
        {
        }
        /// <summary>
        /// 售出后通知玩家
        /// </summary>
        [WZPath("info/soldInform")]
        public bool NotifyWhenSold { get; set; }
    }
}

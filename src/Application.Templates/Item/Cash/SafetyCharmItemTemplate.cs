namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 513
    /// </summary>
    [GenerateTag]
    public class SafetyCharmItemTemplate : CashItemTemplate
    {
        public SafetyCharmItemTemplate(int templateId) : base(templateId)
        {
        }

        /// <summary>
        /// 回复比例
        /// </summary>
        [WZPath("info/recoveryRate")]
        public int RecoveryRate { get; set; }
    }
}

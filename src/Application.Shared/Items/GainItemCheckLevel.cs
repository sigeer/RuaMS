namespace Application.Shared.Items
{
    public enum GainItemCheckLevel
    {
        /// <summary>
        /// 尽可能多的获取物品直至背包空间不足
        /// </summary>
        None,
        /// <summary>
        /// 必须能获取全部物品
        /// </summary>
        Strict,
    }
}

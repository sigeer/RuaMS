namespace Application.Shared.Items
{
    /// <summary>
    /// 拍卖行（MTS / ITC）操作的返回码
    /// </summary>
    public enum MTSResponseCode
    {
        Success = 0,

        /// <summary>
        /// 商品不存在或已被取走
        /// </summary>
        NotFound,
        /// <summary>
        /// 不能购买自己上架的商品
        /// </summary>
        Buy_CannotBuyOwn,
        /// <summary>
        /// 抵用券（NX Prepaid）不足
        /// </summary>
        Buy_CostNotEnough,

        /// <summary>
        /// 试图操作别人的商品
        /// </summary>
        NoAccess,
        /// <summary>
        /// 上架数量已达上限
        /// </summary>
        LimitReached,
        /// <summary>
        /// 请求参数不合法
        /// </summary>
        InvalidRequest,

        /// <summary>
        /// 拍卖出价不高于当前最高价
        /// </summary>
        Bid_TooLow,
        /// <summary>
        /// 自己已经是最高出价者，不能连续加价（客户端 op 60 的文案）
        /// </summary>
        Bid_Consecutive,

        /// <summary>
        /// 该商品正在被领取（并发的第二笔请求，或占位已过期）
        /// </summary>
        InProgress,

        /// <summary>
        /// 背包放不下（中心服侧领取失败，物品仍留在待领取列表）
        /// </summary>
        InventoryFull,

        /// <summary>
        /// 等级不足（客户端规则：10 级以上才能上架）
        /// </summary>
        LevelTooLow,
        /// <summary>
        /// 金币不足（上架手续费）
        /// </summary>
        NotEnoughMeso,
        /// <summary>
        /// 物品不可交易（不可交易标记 / 不可出售）
        /// </summary>
        ItemNotTradable,
    }
}

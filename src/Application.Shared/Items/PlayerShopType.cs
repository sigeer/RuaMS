namespace Application.Shared.Items
{
    public enum PlayerShopType
    {
        PlayerShop,
        HiredMerchant
    }

    public enum PlayerShopStatus
    {
        Opening,
        Maintenance,
    }

    public enum SyncPlayerShopOperation
    {
        Update,
        UpdateByTrade,
        Open,
        Close
    }

    public enum PlayerHiredMerchantStatus
    {
        /// <summary>
        /// 可以开启： 没有开店+没有未领取的物品
        /// </summary>
        Available = 0,
        /// <summary> 
        /// 不可开启：开店了
        /// </summary>
        Unavailable_Opening = 1,
        /// <summary>
        /// 不可开启：没有开店 但是又未领取的物品
        /// </summary>
        Unavailable_NeedRetrieve = 2
    }
}

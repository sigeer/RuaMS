namespace Application.Shared.Events
{
    public enum SyncCharacterTrigger
    {
        Unknown = 0,
        Logoff,
        // 商城服务器
        EnterCashShop,
        /// <summary>
        /// 进入频道服务器前（切换频道、离开商城）
        /// </summary>
        PreEnterChannel,
        LevelChanged,
        JobChanged,
        Auto,
        System,
        /// <summary>
        /// 清理过期数据
        /// </summary>
        ClearExpired,
    }
}

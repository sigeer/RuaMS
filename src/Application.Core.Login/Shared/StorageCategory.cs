namespace Application.Core.Login.Shared
{
    public enum StorageCategory : byte
    {
        Account,
        /// <summary>
        /// 账户共享的游戏数据
        /// </summary>
        AccountGame,

        Character,

        AccountHistory,
        Ban,
        Reward,
        Duey,
        Note,
        Guild,
        BBS,
        Alliance,
        Ring,
        Gachapon,
        PLife,
        /// <summary>
        /// 包含个人商店和雇佣商店
        /// </summary>
        PlayerShop,
        NewYearCard,
        Gift,
        Marriage,

        ExpeditionRecord
    }
}

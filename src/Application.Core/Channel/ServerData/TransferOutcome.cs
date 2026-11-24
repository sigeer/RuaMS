namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 一次物品转移的结局
    /// </summary>
    public enum TransferOutcome
    {
        /// <summary>
        /// 三个阶段都走完，物品已经落到该去的地方
        /// </summary>
        Done,
        /// <summary>
        /// 中心服拒绝了这次转移（不存在 / 没权限 / 超限 / 钱不够…），本地什么都没改
        /// </summary>
        Rejected,
        /// <summary>
        /// 中心服同意了，但本地背包操作失败，已回滚并撤销占位
        /// </summary>
        LocalFailed,
        /// <summary>
        /// 提交重试若干次仍然连不上中心服：结果未知。本地结果被保留（不删玩家的东西），
        /// 靠物品在中心服待领取列表里的那一份人工对账。
        /// </summary>
        Unknown,
    }
}

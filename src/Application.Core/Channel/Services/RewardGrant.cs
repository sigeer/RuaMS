namespace Application.Core.Channel.Services
{
    /// <summary>
    /// 一次奖励<b>实际</b>发到玩家身上的东西。
    /// <para>
    /// 奖励包可能是「道具 + 抵用券 + 点券 + 金币」的混合体，发放过程中任何一步失败都要能原样收回，
    /// 所以边发边记；<see cref="ItemService"/> 在提交被拒时按它回滚。
    /// </para>
    /// </summary>
    internal sealed class RewardGrant
    {
        /// <summary>
        /// 进现金仓的道具。现金仓在中心服，所以这里存的是轻量条目：
        /// 提交成功后才一起送上去（见 <c>ItemService.PushRewardCashItems</c>）。
        /// </summary>
        public List<ProtoModel.ItemProto> CashItems { get; } = [];
        /// <summary>
        /// 进普通背包的道具（回滚时按 itemId + 数量移除）
        /// </summary>
        public List<(int ItemId, short Quantity)> Items { get; } = [];

        public int Meso { get; private set; }
        public int MaplePoint { get; private set; }
        public int NxCredit { get; private set; }
        public int NxPrepaid { get; private set; }

        public void AddMeso(int quantity) => Meso += quantity;
        public void AddMaplePoint(int quantity) => MaplePoint += quantity;
        public void AddNxCredit(int quantity) => NxCredit += quantity;
        public void AddNxPrepaid(int quantity) => NxPrepaid += quantity;
    }
}

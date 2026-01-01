using Application.Core.Channel;
using Application.Core.Game.Maps;

namespace Application.Core.Game.Trades
{
    public interface IPlayerShop : IMapObject
    {
        /// <summary>
        /// 店主（雇佣商人时仅在店主访问时存在）
        /// </summary>
        IPlayer? Owner { get; }
        WorldChannel ChannelServer { get; }
        long StartTime { get; }
        long ExpirationTime { get; }
        string Title { get; }
        int Channel { get; }
        int OwnerId { get; }
        string OwnerName { get; }
        int Mesos { get; }
        int SourceItemId { get; }
        List<PlayerShopItem> Commodity { get; }
        List<SoldItem> SoldHistory { get; }
        HashSet<string> BlackList { get; }
        AtomicEnum<PlayerShopStatus> Status { get; set; }
        PlayerShopType Type { get; }
        /// <summary>
        /// 是否店主
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        bool IsOwner(IPlayer chr);
        bool VisitShop(IPlayer chr);
        void RemoveVisitor(IPlayer visitor);
        bool hasItem(int itemid);
        bool AddCommodity(PlayerShopItem item);


        bool TradeLock();
        void TradeUnlock();

        void GainMeso(int meso);
        string? MesoCheck(int meso);
        void InsertSoldHistory(int idx, SoldItem soldItem);

        void OnCommoditySellout();
        /// <summary>
        /// 对访问者发送更新报
        /// </summary>
        void BroadcastShopItemUpdate();

        Task takeItemBack(int slotIndex, IPlayer chr);

        List<PlayerShopItem> QueryAvailableBundles(int itemid);
        /// <summary>
        /// 物品直接返回到店主的背包里
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        bool Retrieve(IPlayer owner);
        void Close();
        void SetOpen();
        void SetMaintenance(IPlayer chr);

        void sendMessage(IPlayer fromChr, string msg);
    }
}

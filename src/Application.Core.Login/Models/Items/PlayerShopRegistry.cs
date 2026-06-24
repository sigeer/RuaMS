using Application.Core.Login.Shared;
using Application.Shared.Items;
using Application.Utility;

namespace Application.Core.Login.Models.Items
{
    public class PlayerShopRegistry : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Channel { get; set; }
        public int MapId { get; set; }
        public PlayerShopType Type { get; set; }
        public int MapObjectId { get; set; }

        public List<PlayerShopItemModel> Items { get; set; } = [];
        public int Meso { get; set; }
    }

    public class FredrickStoreModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int Cid { get; set; }
        public int Daynotes { get; set; }

        /// <summary>
        /// 存放时间（雇佣店铺关闭时间）
        /// </summary>
        public long StoreTime { get; init; }

        public ItemModel[] Items { get; set; } = [];

        /// <summary>
        /// 交易额
        /// </summary>
        public int Meso { set; get; }
        /// <summary>
        /// 所有商品的总价值
        /// <para>按照客户端的描述，每日手续费还包括原价的1%（目前并没有实现，但是增加这个字段记录）</para>
        /// PS: 经过#r24小时#k便开始征收每日销售金额与物品原价 1%的手续费\r\n5)当手续费超过100%时，便将此充公用作商店街发展委员会的经费
        /// </summary>
        public int ItemMeso { get; set; }

        /// <summary>
        /// 手续费比率
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public int GetFeePercentage(long currentTime)
        {
            var elapsedDays = TimeUtils.TotalDayDiff(StoreTime, currentTime);
            if (elapsedDays <= 1)
            {
                return 0;
            }

            return Math.Max((int)Math.Ceiling(elapsedDays - 1), 100);
        }
    }
}

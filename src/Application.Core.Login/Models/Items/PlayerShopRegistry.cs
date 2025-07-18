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

        public List<PlayerShopItemModel> Items { get; set; } = [];
        public int Meso { get; set; }
    }

    public class FredrickStoreModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int Cid { get; set; }
        public int Daynotes { get; set; }

        public long UpdateTime { get; set; }

        public ItemModel[] Items { get; set; } = [];
        public int Meso { get; set; }
        /// <summary>
        /// 所有商品的总价值
        /// <para>按照客户端的描述，每日手续费还包括原价的1%（目前并没有实现，但是增加这个字段记录）</para>
        /// </summary>
        public long ItemMeso { get; set; }

        public int GetMerchantNetMeso(long currentTime)
        {
            int elapsedDays = TimeUtils.DayDiff(UpdateTime, currentTime);
            if (elapsedDays > 100)
            {
                elapsedDays = 100;
            }

            long netMeso = Meso; // negative mesos issues found thanks to Flash, Vcoc
            netMeso = (netMeso * (100 - elapsedDays)) / 100;
            return (int)netMeso;
        }
    }
}

namespace Application.Host.Models
{
    public class AccountQuery: Pagination
    {
        /// <summary>
        /// 0 未封禁 1. 已封禁
        /// </summary>
        public int Ban { get; set; }
    }

    public class AccountBanQuery : Pagination
    {
        /// <summary>
        /// 0 未封禁 1. 已封禁
        /// </summary>
        public int Ban { get; set; }
    }

    public class MobDropQuery : Pagination
    {
        public int MobId { get; set; }
        public int? ItemId { get; set; }
        public int QuestId { get; set; }
    }

    public class GlobalDropQuery: Pagination
    {
        public int ContinentId { get; set; }
        public int? ItemId { get; set; }
        public int QuestId { get; set; }
    }

    public class ReactorDropQuery: Pagination
    {
        public int? ItemId { get; set; }
        public int QuestId { get; set; }
    }

    public class ShopQuery : Pagination
    {
        public int? ItemId { get; set; }
    }

    public class GachaponQuery
    {
        public int? ItemId { get; set; }
    }
}

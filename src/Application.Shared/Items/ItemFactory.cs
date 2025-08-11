using Application.Utility;

namespace Application.Shared.Items
{
    /**
     * @author Flav
     */
    public class ItemFactory : EnumClass
    {
        /// <summary>
        /// 背包（已装备）
        /// </summary>
        public static readonly ItemFactory INVENTORY = new ItemFactory(ItemType.Inventory, false);
        /// <summary>
        /// 仓库
        /// </summary>
        public static readonly ItemFactory STORAGE = new ItemFactory(ItemType.Storage, true);
        /// <summary>
        /// 现金道具仓库？
        /// </summary>
        public static readonly ItemFactory CASH_EXPLORER = new ItemFactory(ItemType.CashExplorer, true);
        public static readonly ItemFactory CASH_CYGNUS = new ItemFactory(ItemType.CashCygnus, true);
        public static readonly ItemFactory CASH_ARAN = new ItemFactory(ItemType.CashAran, true);
        /// <summary>
        /// 雇佣商人
        /// </summary>
        public static readonly ItemFactory MERCHANT = new ItemFactory(ItemType.Merchant, false);
        public static readonly ItemFactory CASH_OVERALL = new ItemFactory(ItemType.CashOverall, true);
        public static readonly ItemFactory MARRIAGE_GIFTS = new ItemFactory(ItemType.MarriageGift, false);
        /// <summary>
        /// 快递
        /// </summary>
        public static readonly ItemFactory DUEY = new(ItemType.Duey, false);
        /// <summary>
        /// MTS
        /// </summary>
        public static readonly ItemFactory MTS = new(ItemType.MTS, false);
        private ItemType value;
        public bool IsAccount { get; }

        private static int lockCount = 400;
        private static object[] locks = new object[lockCount];  // thanks Masterrulax for pointing out a bottleneck issue here

        static ItemFactory()
        {
            for (int i = 0; i < lockCount; i++)
            {
                locks[i] = new object();
            }
        }

        ItemFactory(ItemType value, bool account)
        {
            this.value = value;
            this.IsAccount = account;
        }

        //public static ItemFactory GetItemFactory(int intValue)
        //{
        //    var value()
        //    if (value == ItemType.Inventory)
        //        return INVENTORY;
        //    if (value == 2)
        //        return STORAGE;
        //    if (value == 3)
        //        return CASH_EXPLORER;
        //    if (value == 4)
        //        return CASH_CYGNUS;
        //    if (value == 5)
        //        return CASH_ARAN;
        //    if (value == 6)
        //        return MERCHANT;
        //    if (value == 7)
        //        return CASH_OVERALL;
        //    if (value == 8)
        //        return MARRIAGE_GIFTS;
        //    if (value == 9)
        //        return DUEY;
        //    throw new BusinessFatalException($"不存在的道具分类 {value}");
        //}

        public int getValue()
        {
            return (int)value;
        }
    }
}

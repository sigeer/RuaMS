using Application.Utility;

namespace Application.Shared.Items
{
    public enum ItemType
    {
        Inventory = 1,
        Storage,
        CashExplorer,
        CashCygnus,
        CashAran,
        Merchant,
        CashOverall,
        MarriageGift,
        Duey
    }

    //public class ItemFactoryType : EnumClass
    //{
    //    /// <summary>
    //    /// 背包（已装备）
    //    /// </summary>
    //    public static readonly ItemFactoryType INVENTORY = new ItemFactoryType(1, false);
    //    /// <summary>
    //    /// 仓库
    //    /// </summary>
    //    public static readonly ItemFactoryType STORAGE = new ItemFactoryType(2, true);
    //    /// <summary>
    //    /// 现金道具仓库？
    //    /// </summary>
    //    public static readonly ItemFactoryType CASH_EXPLORER = new ItemFactoryType(3, true);
    //    public static readonly ItemFactoryType CASH_CYGNUS = new ItemFactoryType(4, true);
    //    public static readonly ItemFactoryType CASH_ARAN = new ItemFactoryType(5, true);
    //    /// <summary>
    //    /// 雇佣商人
    //    /// </summary>
    //    public static readonly ItemFactoryType MERCHANT = new ItemFactoryType(6, false);
    //    public static readonly ItemFactoryType CASH_OVERALL = new ItemFactoryType(7, true);
    //    public static readonly ItemFactoryType MARRIAGE_GIFTS = new ItemFactoryType(8, false);
    //    /// <summary>
    //    /// 快递
    //    /// </summary>
    //    public static readonly ItemFactoryType DUEY = new(9, false);
    //    /// <summary>
    //    /// MTS
    //    /// </summary>
    //    public static readonly ItemFactoryType MTS = new(10, false);
    //    private int value;
    //    private bool account;

    //    ItemFactoryType(int value, bool account)
    //    {
    //        this.value = value;
    //        this.account = account;
    //    }
    //}
}

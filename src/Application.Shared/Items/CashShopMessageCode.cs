namespace Application.Shared.Items
{
    /// <summary>
    /// 商城的提示码（op <c>CASHSHOP_OPERATION</c> 0x5C 后面那个 byte）。
    /// <para>
    /// 完整表见 <c>PacketCreator.showCashShopMessage</c> 上方的注释（客户端文案索引）；
    /// 这里只列出服务端会主动回的几个。
    /// </para>
    /// </summary>
    public class CashShopMessage
    {
        /// <summary>0xA5：余额不足</summary>
        public const byte NotEnoughCash = 0xA5;
        /// <summary>0xC0：库存不足 / 不可购买</summary>
        public const byte NotEnoughInStock = 0xC0;
        /// <summary>0xC4：生日校验失败</summary>
        public const byte CheckBirthday = 0xC4;

        /// <summary>
        /// 非客户端标准码，仅用于服务端内部回传：
        /// 「装备中的宠物不能放进现金仓」（频道据此发 <c>Cash_PutPetIntoCashInventory</c> 文案）
        /// </summary>
        public const int PetEquipped = -1;
        /// <summary>
        /// 非客户端标准码，仅用于服务端内部回传：
        /// 「关系物品不能放进现金仓」（频道据此发 <c>Cash_PutRelationshipItemIntoCashInventory</c> 文案）
        /// </summary>
        public const int RelationshipItem = -2;
    }
}

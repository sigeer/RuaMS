namespace Application.Core.Client.inventory
{
    public record UseItemAction(short Quantity);

    public enum UseItemCheck
    {
        Success = 0,

        /// <summary>
        /// 数量不足
        /// </summary>
        QuantityNotEnough,
        /// <summary>
        /// 道具正在使用中
        /// </summary>
        InProgressing,
        /// <summary>
        /// 使用道具的其他条件不满足
        /// </summary>
        NotPass
    }
}

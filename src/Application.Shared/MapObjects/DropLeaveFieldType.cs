namespace Application.Shared.MapObjects
{
    public enum DropLeaveFieldType
    {
        /// <summary>
        /// 渐隐
        /// </summary>
        Expired = 0,
        /// <summary>
        /// 无动画？
        /// </summary>
        None = 1,
        /// <summary>
        /// 玩家拾取
        /// </summary>
        PickupByPlayer = 2,
        /// <summary>
        /// 金钱炸弹
        /// </summary>
        Explode = 4,
        /// <summary>
        /// 宠物拾取
        /// </summary>
        PickupByPet = 5
    }
}

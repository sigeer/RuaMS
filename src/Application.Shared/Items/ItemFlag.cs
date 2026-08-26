namespace Application.Shared.Items
{
    [Flags]
    public enum ItemFlag : short
    {
        Empty,
        /// <summary>
        /// 封印之锁
        /// </summary>
        LOCK = 0x01,
        /// <summary>
        /// 防滑
        /// </summary>
        SPIKES = 0x02,
        /// <summary>
        /// 使用宿命剪刀后允许交易
        /// </summary>
        KARMA_USE = 0x02,
        /// <summary>
        /// 保暖
        /// </summary>
        COLD = 0x04,
        /// <summary>
        /// 不可交易
        /// </summary>
        UNTRADEABLE = 0x08,
        /// <summary>
        /// 使用宿命剪刀后允许交易
        /// </summary>
        KARMA_EQP = 0x10,

        SANDBOX = 0x40,
        /// <summary>
        /// 仅同账号共享
        /// </summary>
        ACCOUNT_SHARING = 0x100,
    }
}

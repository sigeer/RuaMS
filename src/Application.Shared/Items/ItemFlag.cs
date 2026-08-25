namespace Application.Shared.Items
{
    [Flags]
    public enum ItemFlag : short
    {
        Empty,
        LOCK = 0x01,
        KARMA_PET = 0x01,
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
        UNTRADEABLE = 0x08,
        /// <summary>
        /// 使用宿命剪刀后允许交易
        /// </summary>
        KARMA_EQP = 0x10,
        SANDBOX = 0x40,
        PET_COME = 0x80,
        ACCOUNT_SHARING = 0x100,
        MERGE_UNTRADEABLE = 0x200,
    }
}

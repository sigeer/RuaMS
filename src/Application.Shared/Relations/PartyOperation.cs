namespace Application.Shared.Relations
{
    public enum PartyOperation
    {
        /// <summary>
        /// 加入
        /// </summary>
        JOIN,
        /// <summary>
        /// 离开
        /// </summary>
        LEAVE,
        /// <summary>
        /// 请离
        /// </summary>
        EXPEL,
        /// <summary>
        /// 解散
        /// </summary>
        DISBAND,
        /// <summary>
        /// 更新
        /// </summary>
        SILENT_UPDATE,
        /// <summary>
        /// 离线
        /// </summary>
        LOG_ONOFF,
        /// <summary>
        /// 更换队长
        /// </summary>
        CHANGE_LEADER
    }
}

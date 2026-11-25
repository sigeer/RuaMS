namespace Application.Shared.Events
{
    /// <summary>
    /// 频道向中心服同步角色数据的原因。
    /// <para>
    /// 数值会随 gRPC 跨进程传输，所以中间废弃的枚举留空不重排。
    /// </para>
    /// </summary>
    public enum SyncCharacterTrigger
    {
        Unknown = 0,
        // 离开频道
        LeaveChannel = 1,
        LevelChanged = 8,
        JobChanged = 9,
        Auto = 10,
        System = 11,
        /// <summary>
        /// 清理过期数据
        /// </summary>
        ClearExpired = 12,
    }
}

namespace Application.Core.scripting.Events.Abstraction
{
    public enum TerminationReason : byte
    {
        /// <summary>
        /// 全部完成
        /// </summary>
        Clear,
        /// <summary>
        /// 任务失败（超时、人数不足某种程度也是任务失败）
        /// </summary>
        Failure,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout,
        /// <summary>
        /// 人数不足
        /// </summary>
        MemberCount
    }
}

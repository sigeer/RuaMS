namespace Application.Core.scripting.Events.Abstraction
{
    public enum CreateInstanceResult
    {
        Success = 0,
        RequiredParty,
        /// <summary>
        /// 需要队长来开启
        /// </summary>
        RequiredLeader,
        /// <summary>
        /// 开启限制（等级、人数）
        /// </summary>
        Requirement,
        /// <summary>
        /// 频道数量限制
        /// </summary>
        LobbyLimited,
        Disposed,
        Unknown
    }
}

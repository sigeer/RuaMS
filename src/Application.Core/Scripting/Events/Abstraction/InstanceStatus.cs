using Application.Core.scripting.Events.Instances;

namespace Application.Core.scripting.Events.Abstraction
{
    public enum InstanceStatus
    {
        /// <summary>
        /// 招募中 <see cref="BehindPartyQuestEventInstanceManager"/>
        /// </summary>
        Recruitment,
        /// <summary>
        /// 开始前等待 <see cref="MonsterCarnivalEventInstanceManager"/>
        /// </summary>
        Prepare,
        /// <summary>
        /// 执行中
        /// </summary>
        InProgress,
        /// <summary>
        /// 已全部通关
        /// </summary>
        Cleared,
        /// <summary>
        /// 已释放
        /// </summary>
        Disposed
    }
}

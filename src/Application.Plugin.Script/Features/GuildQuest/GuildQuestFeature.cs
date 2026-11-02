namespace Application.Plugin.Script.Features.GuildQuest
{
    /// <summary>
    /// 公会任务（Guild Quest）。
    /// 通关由阶段脚本调用 clearPQ 触发；额外依赖两段计时器：集合等待 → 正式挑战 → 结束奖励段。
    /// 各层传送门负责记录队伍成员进入的楼层，集合口按计时放行。
    /// </summary>
    internal static class GuildQuestFeature
    {
        internal const string EventName = "GuildQuest";

        #region 传送门

        #endregion

        #region NPC

        #endregion

    }
}

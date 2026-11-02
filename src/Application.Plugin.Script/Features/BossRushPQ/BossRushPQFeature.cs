namespace Application.Plugin.Script.Features.BossRushPQ
{
    /// <summary>
    /// Boss 连战组队任务（Boss Rush PQ）。
    /// 对应 JS scripts/event/BossRushPQ.js。
    /// 入场地图按大厅编号偏移（entryMap + lobby），通关由阶段脚本调用 clearPQ 触发。
    /// </summary>
    internal static class BossRushPQFeature
    {
        internal const string EventName = "BossRushPQ";

        internal const int RecruitMapId = 970030000;

        #region NPC

        #endregion

    }
}

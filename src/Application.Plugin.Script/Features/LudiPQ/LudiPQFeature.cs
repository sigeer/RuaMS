namespace Application.Plugin.Script.Features.LudiPQ
{
    /// <summary>
    /// 组队任务：玩具城（Ludibrium PQ）。
    /// 事件模板与八个阶段传送门原本分处 Events/PartyQuest/PQ_Ludi.cs 与 PortalScript.cs，
    /// 传送门依赖 eim 的 ClearedMaps 状态推进。
    /// </summary>
    internal static class LudiPQFeature
    {
        internal const string EventName = "PQ_Ludi";

        /// <summary>
        /// 阶段地图：922010100 为入口，之后每段 +100，奖励地图为 922011000。
        /// </summary>
        internal const int FirstStageMap = 922010100;
        internal const int BonusMap = 922011000;

        #region NPC

        #endregion

    }
}

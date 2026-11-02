namespace Application.Plugin.Script.Features.KerningPQ
{
    /// <summary>
    /// 组队任务：废弃都市（Kerning PQ）。
    /// 事件、入口/关卡 NPC、五个阶段传送门原本分散在 Events/PartyQuest/PQ_Kerning.cs、
    /// Npc/PartyQuest/PQ_Kerning.cs、PortalScript.cs 三处。
    /// </summary>
    internal static class KerningPQFeature
    {
        internal const string EventName = "PQ_Kerning";

        internal const int Stage1Map = 103000800;
        internal const int Stage2Map = 103000801;
        internal const int Stage3Map = 103000802;
        internal const int Stage4Map = 103000803;
        internal const int Stage5Map = 103000804;
        internal const int BonusMap = 103000805;
        internal const int ExitMapId = 103000890;

        /// <summary>
        /// 通行证：队长上缴数量，以及第五阶段需要上缴的总数。
        /// </summary>
        internal const int PassItem = 4001008;
        internal const int BonusPassCount = 10;

        /// <summary>
        /// 关卡里用于答题的凭据。
        /// </summary>
        internal const int TicketItem = 4001007;

        #region 传送门

        #endregion

        #region NPC

        #endregion
    }
}

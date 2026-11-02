namespace Application.Plugin.Script.Features.RescueGaga
{
    /// <summary>
    /// 营救加加单人挑战。
    /// 对应 JS scripts/event/RescueGaga.js。
    /// 通关条件是走进终点地图；失败掉落超过 3 次会被送回出口。
    /// </summary>
    internal static class RescueGagaFeature
    {
        internal const string EventName = "RescueGaga";

        internal const int EXIT_MAP = 922240200;

        /// <summary>
        /// 允许掉落的次数，超过后送回出口。
        /// </summary>
        internal const int MaxFalls = 3;

        #region NPC

        #endregion

    }
}

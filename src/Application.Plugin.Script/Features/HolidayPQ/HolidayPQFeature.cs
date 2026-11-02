namespace Application.Plugin.Script.Features.HolidayPQ
{
    /// <summary>
    /// 假日组队任务（Maplemas PQ），3 个难度共用同一套规则，仅地图组与等级段不同。
    /// 对应 JS scripts/event/HolidayPQ_{1..3}.js。
    /// 玩法：雪人靠「雪之力量」长大，假道具会让它缩小；雪人成长到顶级后由玩家击败斯克鲁奇通关。
    /// </summary>
    internal static class HolidayPQFeature
    {
        /// <summary>
        /// 各难度的事件名（<c>HolidayPQ_1</c> ~ <c>HolidayPQ_3</c>）。
        /// 事件名由难度号派生，入口 NPC 也是按地图号算出难度再查表。
        /// </summary>
        internal static string EventNameOf(int difficulty) => $"HolidayPQ_{difficulty}";
    }
}

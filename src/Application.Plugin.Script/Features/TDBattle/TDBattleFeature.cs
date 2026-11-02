namespace Application.Plugin.Script.Features.TDBattle
{
    /// <summary>事件模板组：TDBattle（由 Events/TD_Battle.cs 迁入）。</summary>
    internal static class TDBattleFeature
    {
        /// <summary>
        /// 五个关卡的事件名（<c>TD_Battle1</c> ~ <c>TD_Battle5</c>），索引 0 对应 TD_Battle1。
        /// 入口传送门按地图号算出关卡序号再查表。
        /// </summary>
        internal static readonly string[] EventNames =
        [
            TD_Battle1.EventName,
            TD_Battle2.EventName,
            TD_Battle3.EventName,
            TD_Battle4.EventName,
            TD_Battle5.EventName,
        ];
    }
}

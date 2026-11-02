namespace Application.Plugin.Script.Features.PQ_Ariant
{
    /// <summary>
    /// 阿里安特竞技场。
    /// </summary>
    internal static class PQ_AriantFeature
    {
        /// <summary>三个房间的事件名（<c>PQ_Ariant1</c> ~ <c>PQ_Ariant3</c>），索引 0 对应 Room1。</summary>
        internal static readonly string[] EventNames =
        [
            Room1.EventName,
            Room2.EventName,
            Room3.EventName,
        ];
    }
}

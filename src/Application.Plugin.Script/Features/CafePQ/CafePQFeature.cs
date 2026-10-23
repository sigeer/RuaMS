namespace Application.Plugin.Script.Features.CafePQ
{
    /// <summary>事件模板组：CafePQ（由 Events/CafePQ.cs 迁入）。</summary>
    internal static class CafePQFeature
    {
        /// <summary>六个难度的事件名（<c>CafePQ_1</c> ~ <c>CafePQ_6</c>）。</summary>
        internal static readonly string[] EventNames =
        [
            CafePQ_1.EventName,
            CafePQ_2.EventName,
            CafePQ_3.EventName,
            CafePQ_4.EventName,
            CafePQ_5.EventName,
            CafePQ_6.EventName,
        ];
    }
}

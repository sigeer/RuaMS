namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 玛加提亚组队任务：Alcadno（<c>jnr*</c>）与 Zenumist（<c>rnj*</c>）两套传送门结构相同，
    /// 只有地图号基数与事件名不同。这里用同一套实现，进入时按当前事件决定基数。
    /// </summary>
    internal static class MagatiaPQFeature
    {
        internal const string AlcadnoEventName = "MagatiaPQ_A";
        internal const string ZenumistEventName = "MagatiaPQ_Z";

        internal const int AlcadnoMapBase = 926110000;
        internal const int ZenumistMapBase = 926100000;

    }
}

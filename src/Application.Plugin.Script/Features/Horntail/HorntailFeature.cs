namespace Application.Plugin.Script.Features.Horntail
{
    /// <summary>
    /// 火焰龙（Horntail）：远征（<c>Exped_Horntail</c>）、试炼组队任务（<c>PQ_Horntail</c>）
    /// 以及两者共用的封印门、脑袋传送门与入口 NPC 集中在此功能内。
    /// 封印开启条件写在 eim 的 <c>Nstageclear</c> / <c>defeatedHead</c> 属性上。
    /// </summary>
    internal static class HorntailFeature
    {
        /// <summary>远征事件注册名。</summary>
        internal const string ExpeditionEventName = "Exped_Horntail";

        /// <summary>试炼组队任务事件注册名。</summary>
        internal const string PartyQuestEventName = "PQ_Horntail";
    }
}

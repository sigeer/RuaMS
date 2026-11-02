namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>
    /// 怪物嘉年华：两代（CPQ1 地图组 980000xxx、CPQ2 地图组 980031xxx~980033xxx），
    /// 每代三个房间共用同一套规则，仅招募地图与等级段不同。
    /// 入口 NPC 同时服务两代，因此集中在同一功能内。
    /// 房间模板见 <see cref="Cpq1Room1"/> 等六个类。
    /// </summary>
    internal static class MonsterCarnivalFeature
    {
        /// <summary>怪物嘉年华 1 的出口地图。</summary>
        internal const int Cpq1ExitMap = 980000010;

        /// <summary>怪物嘉年华 2 的出口地图。</summary>
        internal const int Cpq2ExitMap = 980030010;

        /// <summary>嘉年华 1 三个房间的事件名，索引 0 对应 Cpq1Room1。</summary>
        internal static readonly string[] Cpq1EventNames =
        [
            Cpq1Room1.EventName,
            Cpq1Room2.EventName,
            Cpq1Room3.EventName,
        ];

        /// <summary>嘉年华 2 三个房间的事件名，索引 0 对应 Cpq2Room1。</summary>
        internal static readonly string[] Cpq2EventNames =
        [
            Cpq2Room1.EventName,
            Cpq2Room2.EventName,
            Cpq2Room3.EventName,
        ];
    }
}

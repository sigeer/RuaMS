namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 1 号房（招募地图 980000100，等级 30~50）。</summary>
    internal sealed class Cpq1Room1 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ11";

        public Cpq1Room1() : base(EventName, 980000100, MonsterCarnivalFeature.Cpq1ExitMap, 30, 50) { }
    }
}

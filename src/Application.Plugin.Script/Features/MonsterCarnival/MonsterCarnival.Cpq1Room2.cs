namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 1 号房（招募地图 980000200，等级 30~50）。</summary>
    internal sealed class Cpq1Room2 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ12";

        public Cpq1Room2() : base(EventName, 980000200, MonsterCarnivalFeature.Cpq1ExitMap, 30, 50) { }
    }
}

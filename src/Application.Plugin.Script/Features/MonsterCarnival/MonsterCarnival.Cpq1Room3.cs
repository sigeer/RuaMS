namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 1 号房（招募地图 980000300，等级 30~50）。</summary>
    internal sealed class Cpq1Room3 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ13";

        public Cpq1Room3() : base(EventName, 980000300, MonsterCarnivalFeature.Cpq1ExitMap, 30, 50) { }
    }
}

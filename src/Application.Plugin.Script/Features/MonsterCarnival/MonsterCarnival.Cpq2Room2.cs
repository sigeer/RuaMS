namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 2 号房（招募地图 980032000，等级 51~70）。</summary>
    internal sealed class Cpq2Room2 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ22";

        public Cpq2Room2() : base(EventName, 980032000, MonsterCarnivalFeature.Cpq2ExitMap, 51, 70) { }
    }
}

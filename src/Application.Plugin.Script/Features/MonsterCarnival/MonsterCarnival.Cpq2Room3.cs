namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 2 号房（招募地图 980033000，等级 51~70）。</summary>
    internal sealed class Cpq2Room3 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ23";

        public Cpq2Room3() : base(EventName, 980033000, MonsterCarnivalFeature.Cpq2ExitMap, 51, 70) { }
    }
}

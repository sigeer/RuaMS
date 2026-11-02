namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>怪物嘉年华 2 号房（招募地图 980031000，等级 51~70）。</summary>
    internal sealed class Cpq2Room1 : MonsterCarnivalRoomTemplate
    {
        internal const string EventName = "PQ_CPQ21";

        public Cpq2Room1() : base(EventName, 980031000, MonsterCarnivalFeature.Cpq2ExitMap, 51, 70) { }
    }
}

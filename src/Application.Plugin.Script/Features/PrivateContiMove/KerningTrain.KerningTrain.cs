namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>废弃都市 ↔ 废都广场。</summary>
    internal sealed class KerningTrain : PrivateContiMoveBase
    {
        public KerningTrain() : base(KerningTrainFeature.EventName, [103000100, 103000310], [103000301, 103000302], 50) { }
    }
}

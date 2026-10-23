namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>天空之城 ↔ 武陵</summary>
    internal sealed class Crane : PrivateContiMoveBase
    {
        public Crane() : base(CraneFeature.EventName,
            [200000141, 250000100],
            [200090300, 200090310],
            60)
        {
        }
    }
}

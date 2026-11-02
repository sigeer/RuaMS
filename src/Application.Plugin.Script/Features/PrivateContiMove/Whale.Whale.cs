using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>里恩 ↔ 明珠港</summary>
    internal sealed class Whale : PrivateContiMoveBase
    {
        public Whale() : base(WhaleFeature.EventName,
            [MapId.DANGEROUS_FOREST, MapId.LITH_HARBOUR],
            [MapId.FROM_RIEN_TO_LITH, MapId.FROM_LITH_TO_RIEN],
            60)
        {
        }
    }
}

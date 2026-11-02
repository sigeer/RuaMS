using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>魔法密林 ↔ 圣地</summary>
    internal sealed class ShipEllin : PrivateContiMoveBase
    {
        public ShipEllin() : base(ShipEllinFeature.EventName,
            [MapId.ELLINIA_SKY_FERRY, MapId.SKY_FERRY],
            [MapId.FROM_ELLINIA_TO_EREVE, MapId.FROM_EREVE_TO_ELLINIA],
            2 * 60)
        {
        }
    }
}

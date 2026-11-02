using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>天空之城 ↔ 圣地</summary>
    internal sealed class ShipOribs : PrivateContiMoveBase
    {
        public ShipOribs() : base(ShipOribsFeature.EventName,
            [MapId.ORBIS_STATION, MapId.SKY_FERRY],
            [200090020, 200090021],
            8 * 60)
        {
        }
    }
}

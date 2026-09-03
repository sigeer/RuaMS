using Application.Core.Game.Maps;
using Application.Core.Server.maps;
using Application.Shared.MapObjects.Portals;

namespace Application.Core.server.maps
{
    public class MysticDoorPortal : GenericPortal
    {
        public DoorObject? Door { get; set; }
        public MysticDoorPortal() : base(PortalConstants.DOOR_PORTAL)
        {
        }

    }
}

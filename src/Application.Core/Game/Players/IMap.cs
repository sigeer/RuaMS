using Application.Core.Game.Maps;
using Application.Core.Server.maps;
using Application.Shared.WzEntity;

namespace Application.Core.Game.Players
{
    public interface IMapPlayer
    {

        void changeMap(int map);
        void changeMap(int map, int portal);
        void changeMap(int map, IPortal portal);
        void changeMap(int map, string portal);
        void changeMap(IMap to, int portal = 0);
        void changeMap(IMap target, Point pos);
        void changeMap(IMap target, IPortal? pto);

        void changeMapBanish(BanishInfo banishInfo);

        /// <summary>
        /// 和<see cref="changeMap(IMap target, IPortal? pto)"/>的区别：不会走EventInstance的MapManager
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pto"></param>
        void forceChangeMap(IMap target, IPortal? pto = null);

        void visitMap(IMap map);

        void startMapEffect(string msg, int itemId, int duration = 30000);

        void showMapOwnershipInfo(Player mapOwner);
        void ForcedWarpOut();
    }
}

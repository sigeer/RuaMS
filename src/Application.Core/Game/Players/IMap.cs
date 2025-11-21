using Application.Core.Game.Maps;
using Application.Shared.WzEntity;
using server.maps;

namespace Application.Core.Game.Players
{
    public interface IMapPlayer
    {

        void changeMap(int map);
        void changeMap(int map, int portal);
        void changeMap(int map, Portal portal);
        void changeMap(int map, string portal);
        void changeMap(IMap to, int portal = 0);
        void changeMap(IMap target, Point pos);
        void changeMap(IMap target, Portal? pto);

        void changeMapBanish(BanishInfo banishInfo);

        /// <summary>
        /// 和 <see cref="changeMap(IMap, Portal?)"/>什么差异？可以跨事件传送？
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pto"></param>
        void forceChangeMap(IMap target, Portal? pto);
        void enteredScript(string script, int mapid);

        void visitMap(IMap map);
        void warpAhead(int map);

        void startMapEffect(string msg, int itemId, int duration = 30000);

        void showMapOwnershipInfo(IPlayer mapOwner);
    }
}

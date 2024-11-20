using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using constants.id;
using Microsoft.EntityFrameworkCore;
using server.maps;

namespace Application.Core.Managers
{
    public class AdminManager
    {


        /// <summary>
        /// <paramref name="player"/>击杀<paramref name="map"/>内所有可攻击的怪物
        /// </summary>
        /// <param name="player"></param>
        /// <param name="map"></param>
        /// <returns>击杀数</returns>
        public static int KillAllMonster(IPlayer player) => KillAllMonster(player, player.getMap());
        /// <summary>
        /// <paramref name="player"/>击杀<paramref name="map"/>内所有可攻击的怪物
        /// </summary>
        /// <param name="player"></param>
        /// <param name="map"></param>
        /// <returns>击杀数</returns>
        public static int KillAllMonster(IPlayer player, IMap map)
        {
            var monsters = map.getMapObjectsInRange(Point.Empty, double.PositiveInfinity, Arrays.asList(MapObjectType.MONSTER));
            int count = 0;
            foreach (var monstermo in monsters)
            {
                Monster monster = (Monster)monstermo;
                if (!monster.getStats().isFriendly() && !(monster.getId() >= MobId.DEAD_HORNTAIL_MIN && monster.getId() <= MobId.HORNTAIL))
                {
                    map.damageMonster(player, monster, int.MaxValue);
                    count++;
                }
            }
            return count;
        }
    }
}

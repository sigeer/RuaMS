using Application.Core.Game.Life;
using Application.Core.Game.Maps;

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
        public static async Task<int> KillAllMonster(Player player) => await KillAllMonster(player, player.getMap());
        /// <summary>
        /// <paramref name="player"/>击杀<paramref name="map"/>内所有可攻击的怪物
        /// </summary>
        /// <param name="player"></param>
        /// <param name="map"></param>
        /// <returns>击杀数</returns>
        public static async Task<int> KillAllMonster(Player player, IMap map)
        {
            var monsters = map.GetMapObjects(x => x.getType() == MapObjectType.MONSTER);
            int count = 0;
            foreach (var monstermo in monsters)
            {
                Monster monster = (Monster)monstermo;
                if (!monster.getStats().isFriendly() && !(monster.getId() >= MobId.DEAD_HORNTAIL_MIN && monster.getId() <= MobId.HORNTAIL))
                {
                    await monster.DamageBy(player, int.MaxValue, 0);
                    count++;
                }
            }
            return count;
        }
    }
}

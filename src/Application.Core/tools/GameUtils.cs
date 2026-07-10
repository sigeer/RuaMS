using Application.Core.Channel.DataProviders;
using Application.Core.Game.Maps;
using Application.Templates.Map;
using Application.Templates.Reader;

namespace Application.Core.tools
{
    internal class GameUtils
    {
        public static int MAX_FIELD_MOB_DAMAGE = getMaxObstacleMobDamage() * 2;

        private static int getMaxObstacleMobDamage()
        {
            var provider = ProviderSource.Instance.GetProvider<IProvider<MapObstacleTemplate>>(ProviderType.MapObstacle);
            return provider.LoadAll().Select(t => t.MobDamage).DefaultIfEmpty(0).Max();
        }

        public static bool isMedalQuest(short questid)
        {
            return QuestFactory.Instance.GetMedalRequirement(questid) != -1;
        }

        public static bool isMerchantLocked(IMap map)
        {
            if (FieldLimit.CANNOTMIGRATE.check(map.getFieldLimit()))
            {   // maps that cannot access cash shop cannot access merchants too (except FM rooms).
                return true;
            }

            return map.getId() == MapId.FM_ENTRANCE;
        }
    }
}

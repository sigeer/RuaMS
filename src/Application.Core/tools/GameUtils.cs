using Application.Core.Game.Maps;
using server.quest;

namespace Application.Core.tools
{
    internal class GameUtils
    {
        public static int MAX_FIELD_MOB_DAMAGE = getMaxObstacleMobDamageFromWz() * 2;

        private static int getMaxObstacleMobDamageFromWz()
        {
            DataProvider mapSource = DataProviderFactory.getDataProvider(WZFiles.MAP);
            int maxMobDmg = 0;

            DataDirectoryEntry root = mapSource.getRoot();
            foreach (DataDirectoryEntry objData in root.getSubdirectories())
            {
                if (objData.getName() != ("Obj"))
                {
                    continue;
                }

                foreach (DataFileEntry obj in objData.getFiles())
                {
                    foreach (Data l0 in mapSource.getData(objData.getName() + "/" + obj.getName()).getChildren())
                    {
                        foreach (Data l1 in l0.getChildren())
                        {
                            foreach (Data l2 in l1.getChildren())
                            {
                                int objDmg = DataTool.getIntConvert("s1/mobdamage", l2, 0);
                                if (maxMobDmg < objDmg)
                                {
                                    maxMobDmg = objDmg;
                                }
                            }
                        }
                    }
                }
            }

            return maxMobDmg;
        }

        public static bool isMedalQuest(short questid)
        {
            return Quest.getInstance(questid).getMedalRequirement() != -1;
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

using Application.Core.EF.Entities.Gachapons;
using Application.Core.tools.RandomUtils;
using Application.EF;
using constants.id;
using Microsoft.EntityFrameworkCore;
using server;
using tools;

namespace Application.Core.Managers
{
    public class GachaponManager
    {
        public static List<GachaponPool> GetCachedGlobalPool()
        {
            using var dbContext = new DBContext();
            return dbContext.GachaponPools.Where(x => x.NpcId == -1).AsNoTracking().ToList();
        }
        public static GachaponRewardInfo DoGachapon(int npcId)
        {
            using var dbContext = new DBContext();
            var pool = dbContext.GachaponPools.Where(x => x.NpcId == npcId || x.NpcId == -1).AsNoTracking().ToList();
            var machine = new LotteryMachine<int>(pool.Select(x => new LotteryMachinItem<int>(x.Id, x.LevelChance)));
            var rank = machine.GetRandomItem();
            var poolObj = pool.FirstOrDefault(x => x.Id == rank)!;
            var poolItem = Randomizer.Select(dbContext.GachaponPoolItems.Where(x => x.PoolId == rank));
            return new GachaponRewardInfo(poolItem.ItemId, poolItem.PoolId, poolObj.Level);
        }

        public static List<GachaponPool> GetByNpcId(int npcId)
        {
            using var dbContext = new DBContext();
            return dbContext.GachaponPools.Where(x => x.NpcId == npcId || x.NpcId == -1).AsNoTracking().ToList();
        }

        public static string[] GetLootInfo()
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            using var dbContext = new DBContext();
            var allGachaponType = dbContext.GachaponPools.Where(x => x.NpcId != -1 && x.Level == 0).ToList();
            string[] strList = new string[allGachaponType.Count + 1];

            string menuStr = "";
            int j = 0;
            foreach (var gacha in allGachaponType)
            {
                menuStr += "#L" + j + "#" + gacha.name() + "#l\r\n";
                j++;

                string str = "";
                for (int i = 0; i < 3; i++)
                {
                    int[] gachaItems = gacha.getItems(i);

                    if (gachaItems.Length > 0)
                    {
                        str += ("  #rTier " + i + "#k:\r\n");
                        foreach (int itemid in gachaItems)
                        {
                            var itemName = ii.getName(itemid);
                            if (itemName == null)
                            {
                                itemName = "MISSING NAME #" + itemid;
                            }

                            str += ("    " + itemName + "\r\n");
                        }

                        str += "\r\n";
                    }
                }
                str += "\r\n";

                strList[j] = str;
            }
            strList[0] = menuStr;

            return strList;
        }

        public static List<int> GetItems(GachaponPool pool)
        {
            using var dbContext = new DBContext();
            return dbContext.GachaponPoolItems.Where(x => x.PoolId == pool.Id).Select(x => x.ItemId).ToList();
        }
    }
}

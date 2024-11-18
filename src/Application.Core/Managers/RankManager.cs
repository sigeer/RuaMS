namespace Application.Core.Managers
{
    public class RankManager
    {


        /// <summary>
        /// 获取区内角色等级前 <paramref name="topCount"/> 名
        /// </summary>
        /// <param name="topCount"></param>
        /// <returns>WorldId - Data -1: 全部</returns>
        public static Dictionary<int, List<RankedCharacterInfo>> LoadPlayerRankingFromDB(int topCount = 50)
        {
            using var dbContext = new DBContext();
            var query = from a in dbContext.Characters
                        join b in dbContext.Accounts on a.AccountId equals b.Id
                        where a.Gm < 2 && b.Banned != 1
                        select a;

            var list = (from a in query
                        orderby a.World, a.Level descending, a.Exp descending, a.LastExpGainTime
                        group a by a.World into bss
                        select new { bss.Key, data = bss.Select(x => new { x.Level, x.Name, x.Exp, x.LastExpGainTime, x.World }).Take(topCount) }).ToList();

            var dic = new Dictionary<int, List<RankedCharacterInfo>>();
            dic[-1] = list.SelectMany(x => x.data)
                .OrderByDescending(x => x.Level)
                .ThenByDescending(x => x.Exp)
                .ThenBy(x => x.LastExpGainTime)
                .Select((x, idx) => new RankedCharacterInfo(idx + 1, x.World, x.Level, x.Name))
                .Take(topCount)
                .ToList();

            foreach (var x in list)
            {
                dic[x.Key] = x.data.Select((y, index) => new RankedCharacterInfo(index + 1, x.Key, y.Level, y.Name)).ToList();
            }
            return dic;
        }
    }
}

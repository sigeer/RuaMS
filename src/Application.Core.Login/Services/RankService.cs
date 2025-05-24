using Application.EF;
using Application.Shared;

namespace Application.Core.Login.Services
{
    public class RankService
    {
        public List<RankedCharacterInfo> LoadPlayerRankingFromDB(DBContext dbContext, int topCount = 50)
        {
            return (from a in dbContext.Characters
                    join b in dbContext.Accounts on a.AccountId equals b.Id
                    where b.GMLevel < 2 && b.Banned != 1
                    orderby a.Level descending, a.Exp descending, a.LastExpGainTime
                    select a).Take(topCount)
                        .Select((x, idx) => new RankedCharacterInfo(idx + 1, 0, x.Level, x.Name)).ToList();
        }
    }
}

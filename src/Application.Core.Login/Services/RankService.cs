using Application.EF;
using AutoMapper;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class RankService
    {
        readonly IDistributedCache _cache;
        readonly IMapper _mapper;
        readonly ILogger<RankService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;

        public RankService(IDistributedCache cache, IMapper mapper, ILogger<RankService> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer server)
        {
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _server = server;
        }
        public Rank.RankCharacterList LoadPlayerRanking(int topCount = 50, bool ignoreCache = false)
        {
            var cacheKey = "Rank";
            if (!ignoreCache && _cache.Get(cacheKey) is byte[] cachedBytes)
            {
                return Rank.RankCharacterList.Parser.ParseFrom(cachedBytes);
            }

            var data = LoadPlayerRankingFromDB(topCount);
            _cache.Set(cacheKey, data.ToByteArray());
            return data;
        }

        private Rank.RankCharacterList LoadPlayerRankingFromDB(int topCount = 50)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var current = _server.GetCurrentTimeDateTimeOffset();
            var dataList = (from a in dbContext.Characters
                            join b in dbContext.Accounts on a.AccountId equals b.Id
                            let bindings = dbContext.AccountBans.Where(x => x.AccountId == a.AccountId)
                            where b.GMLevel < 2 && bindings.Count(x => x.EndTime >= current) == 0
                            orderby a.Level descending, a.Exp descending, a.LastExpGainTime
                            select new { a.Level, a.Name }).Take(topCount).ToList();
            var obj = new Rank.RankCharacterList();
            for (int i = 0; i < dataList.Count; i++)
            {
                var item = dataList[i];
                obj.DataSource.Add(new Rank.RankCharacter { Rank = i + 1, Name = item.Name, Level = item.Level });
            }
            return obj;
        }
    }
}

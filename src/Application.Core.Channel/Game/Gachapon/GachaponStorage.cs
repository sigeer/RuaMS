using Application.Core.EF.Entities.Gachapons;
using Application.Core.tools.RandomUtils;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Gachapon
{
    public class GachaponStorage
    {
        /// <summary>
        /// poolId
        /// </summary>
        private List<int> _globalPoolIdList;
        private GachaponStorage()
        {
            _cachedPool = [];
            _cachedItems = [];

            _cachedChance = [];
            _globalPoolIdList = [];

            LoadDataFromDB();
        }

        private static Lazy<GachaponStorage> _instance = new Lazy<GachaponStorage>(new GachaponStorage());
        public static GachaponStorage Instance => _instance.Value;

        private List<GachaponPoolLevelChance> _cachedChance;
        private List<GachaponPoolItem> _cachedItems;

        /// <summary>
        /// npcid - pool
        /// </summary>
        Dictionary<int, GachaponPool> _cachedPool;

        public void LoadDataFromDB()
        {
            using var dbContext = new DBContext();
            _cachedChance = dbContext.GachaponPoolLevelChances.OrderBy(x => x.PoolId).ThenBy(x => x.Level).AsNoTracking().ToList();
            _cachedPool = dbContext.GachaponPools.AsNoTracking().ToList().ToDictionary(x => x.NpcId);
            _cachedItems = dbContext.GachaponPoolItems.AsNoTracking().ToList();

            _globalPoolIdList = _cachedPool.Where(x => x.Key <= 0).Select(x => x.Value.Id).ToList();
        }

        public List<GachaponPool> GetGachaponType()
        {
            return _cachedPool.Where(x => x.Key > 0).Select(x => x.Value).ToList();
        }

        public void Reset() => LoadDataFromDB();

        public List<GachaponPoolItem> GetItems(int poolId, int level)
        {
            return _cachedItems.Where(x => x.Level == level).Where(x => x.PoolId == poolId || _globalPoolIdList.Contains(x.PoolId)).ToList();
        }

        public GachaponPoolItem DoGachapon(int npcId)
        {
            var pool = GetByNpcId(npcId);
            if (pool == null)
                throw new BusinessResException($"没有找到NPC（{npcId}）的扭蛋机数据");

            var machine = new LotteryMachine<int>(_cachedChance.Where(x => x.PoolId == pool.Id).Select(x => new LotteryMachinItem<int>(x.Level, x.Chance)));
            var level = machine.GetRandomItem();
            return Randomizer.Select(GetItems(pool.Id, level));
        }

        public GachaponPool? GetByNpcId(int npcId)
        {
            return _cachedPool.GetValueOrDefault(npcId);
        }

        public List<GachaponPoolLevelChance> GetPoolLevelList(int poolId)
        {
            return _cachedChance.Where(x => x.PoolId == poolId).ToList();
        }

    }
}

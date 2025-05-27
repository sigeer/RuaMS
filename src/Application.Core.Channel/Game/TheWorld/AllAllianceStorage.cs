using Application.Core.Game.Relation;
using Application.Core.Managers;
using System.Collections.Concurrent;

namespace Application.Core.Game.TheWorld
{
    public class AllAllianceStorage
    {
        private static ConcurrentDictionary<int, IAlliance?> CachedData { get; set; } = new();
        public static IAlliance AddOrUpdate(IAlliance alliance) => CachedData[alliance.AllianceId] = alliance;
        public static IAlliance? GetAllianceById(int id)
        {
            return CachedData.GetOrAdd(id, AllianceManager.loadAlliance);
        }
        public static void Remove(int allianceId)
        {
            CachedData.TryRemove(allianceId, out var _);
        }
    }
}

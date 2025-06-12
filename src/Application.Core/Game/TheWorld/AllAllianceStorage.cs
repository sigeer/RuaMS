using Application.Core.Game.Relation;
using Application.Core.Managers;
using System.Collections.Concurrent;

namespace Application.Core.Game.TheWorld
{
    public class AllAllianceStorage
    {
        private static ConcurrentDictionary<int, Alliance?> CachedData { get; set; } = new();
        public static Alliance AddOrUpdate(Alliance alliance) => CachedData[alliance.AllianceId] = alliance;
        public static void Remove(int allianceId)
        {
            CachedData.TryRemove(allianceId, out var _);
        }
    }
}

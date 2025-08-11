using Application.Core.Game.Relation;
using System.Collections.Concurrent;

namespace Application.Core.Game.TheWorld
{
    public class AllGuildStorage
    {
        private static ConcurrentDictionary<int, Guild?> CachedData { get; set; } = new();
        public static Guild? GetGuildById(int id)
        {
            return null;
            // return CachedData.GetOrAdd(id, GuildManager.FindGuildFromDB);
        }

        public static void Remove(params int[] guildIdArr)
        {
            foreach (var guildId in guildIdArr)
            {
                CachedData.TryRemove(guildId, out var _);
            }
        }
        public static Guild? GetGuildByName(string name)
        {
            return CachedData.Values.FirstOrDefault(x => x != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static void Remove(List<int> guildIdArr)
        {
            foreach (var guildId in guildIdArr)
            {
                CachedData.TryRemove(guildId, out var _);
            }
        }
    }
}

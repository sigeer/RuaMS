using Application.Core.Game.Relation;
using Application.Core.Managers;
using System.Collections.Concurrent;

namespace Application.Core.Game.TheWorld
{
    public class AllGuildStorage
    {
        private static ConcurrentDictionary<int, IGuild?> CachedData { get; set; } = new();
        public static IGuild? GetGuildById(int id)
        {
            return CachedData.GetOrAdd(id, GuildManager.FindGuildFromDB);
        }
        public static IGuild AddOrUpdate(IGuild guild) => CachedData[guild.GuildId] = guild;
        public static void Remove(params int[] guildIdArr)
        {
            foreach (var guildId in guildIdArr)
            {
                CachedData.TryRemove(guildId, out var _);
            }
        }
        public static IGuild? GetGuildByName(string name)
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

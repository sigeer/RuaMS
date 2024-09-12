using Application.Core.Managers;
using System.Collections.Concurrent;

namespace Application.Core.Game.TheWorld
{
    /// <summary>
    /// 所有区、频道的玩家角色都从这里获取（包含了不在线玩家）
    /// </summary>
    public class AllPlayerStorage
    {
        private static ConcurrentDictionary<int, DataLevel> CachedData { get; set; } = new ();
        private static ConcurrentDictionary<string, DataLevel> NamedCacheData { get; set; } = new();
        public static List<IPlayer> GetAllOnlinedPlayers()
        {
            return CachedData.Values.Select(x => x.Data).Where(x => x.IsOnlined).ToList();
        }
        public static void AddOrUpdate(DataLevel dataLevel)
        {
            CachedData[dataLevel.Data.Id] = dataLevel;
            NamedCacheData[dataLevel.Data.Name] = dataLevel;
        }
        public static IPlayer? GetOrAddCharacterById(int id, int level = 0)
        {
            var m = CachedData.GetValueOrDefault(id);
            if (m != null && m.Level >= level)
                return m.Data;

            IPlayer? player = CharacterManager.GetPlayerById(id, level > 0);

            if (player == null)
                return null;

            AddOrUpdate(new DataLevel(level, player));
            return player;
        }

        public static List<IPlayer> GetPlayersByIds(IEnumerable<int> idList, int level = 0)
        {
            List<IPlayer> list = new();

            List<int> notFound = new List<int>();
            foreach (var id in idList)
            {
                var m = CachedData.GetValueOrDefault(id);
                if (m != null && m.Level >= level)
                    list.Add(m.Data);
                else
                    notFound.Add(id);
            }

            var notFoundList = CharacterManager.GetPlayersById(notFound, level > 0);
            foreach (var item in notFoundList)
            {
                AddOrUpdate(new DataLevel(level, item));
                list.Add(item);
            }
            return list;
        }

        public static IPlayer? GetOrAddCharacterByName(string name, int level = 0)
        {
            var m = NamedCacheData.GetValueOrDefault(name);
            if (m != null && m.Level >= level)
                return m.Data;

            IPlayer? player = CharacterManager.GetPlayerByName(name, level > 0);

            if (player == null)
                return null;

            AddOrUpdate(new DataLevel(level, player));
            return player;
        }
        public static List<IPlayer> GetPlayersByNames(IEnumerable<string> nameList, int level = 0)
        {
            List<IPlayer> list = new();

            List<string> notFound = new();
            foreach (var name in nameList)
            {
                var m = NamedCacheData.GetValueOrDefault(name);
                if (m != null && m.Level >= level)
                    list.Add(m.Data);
                else
                    notFound.Add(name);
            }

            var notFoundList = CharacterManager.GetPlayersByName(notFound);
            foreach (var item in notFoundList)
            {
                AddOrUpdate(new DataLevel(level, item));
                list.Add(item);
            }
            return list;
        }

        public static void DeleteCharacter(int id)
        {
            var data = CachedData.GetValueOrDefault(id);
            if (data != null)
            {
                CachedData.Remove(id);
                NamedCacheData.Remove(data.Data.Name);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Level">
    /// <para>0: 最低限度的Player数据</para>
    /// <para>1: 附带佩戴道具的Player数据</para>
    /// <para>2: 登录世界时的Player数据</para>
    /// </param>
    /// <param name="Data">Player数据</param>
    public record DataLevel(int Level, IPlayer Data);
}

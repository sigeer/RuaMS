using Application.Core.Managers;

namespace Application.Core.Game.TheWorld
{
    public class AllPlayerStorage
    {
        private static Dictionary<int, IPlayer> CachedData { get; set; } = new Dictionary<int, IPlayer>();
        private static Dictionary<string, IPlayer> NamedCacheData { get; set; } = new();
        public static List<IPlayer> GetAllOnlinedPlayers()
        {
            return CachedData.Values.Where(x => x.IsOnlined).ToList();
        }
        public static void AddPlayer(IPlayer player)
        {
            CachedData[player.Id] = player;
            NamedCacheData[player.Name] = player;
        }
        public static IPlayer? GetOrAddCharacterById(int id)
        {
            var m = CachedData.GetValueOrDefault(id);
            if (m != null)
                return m;

            m = CharacterManager.GetPlayerById(id);
            if (m != null)
                AddPlayer(m);
            return m;
        }

        public static List<IPlayer> GetPlayersByIds(IEnumerable<int> idList)
        {
            List<IPlayer> list = new();

            List<int> notFound = new List<int>();
            foreach (var id in idList)
            {
                var m = CachedData.GetValueOrDefault(id);
                if (m != null)
                    list.Add(m);
                else
                    notFound.Add(id);
            }

            var notFoundList = CharacterManager.GetPlayersById(notFound);
            foreach (var item in notFoundList)
            {
                AddPlayer(item);
                list.Add(item);
            }
            return list;
        }

        public static IPlayer? GetOrAddCharacterByName(string name)
        {
            var m = NamedCacheData.GetValueOrDefault(name);
            if (m != null)
                return m;

            m = CharacterManager.GetPlayerByName(name);
            if (m != null)
                AddPlayer(m);
            return m;
        }
        public static List<IPlayer> GetPlayersByNames(IEnumerable<string> nameList)
        {
            List<IPlayer> list = new();

            List<string> notFound = new();
            foreach (var name in nameList)
            {
                var m = NamedCacheData[name];
                if (m != null)
                    list.Add(m);
                else
                    notFound.Add(name);
            }

            var notFoundList = CharacterManager.GetPlayersByName(notFound);
            foreach (var item in notFoundList)
            {
                AddPlayer(item);
                list.Add(item);
            }
            return list;
        }
    }
}

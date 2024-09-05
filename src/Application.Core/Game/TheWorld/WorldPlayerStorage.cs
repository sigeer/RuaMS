using Application.Core.Managers;

namespace Application.Core.Game.TheWorld
{
    /// <summary>
    /// 所有关于角色数据加载的都放在这里，如果未登录，则client是offlineclient，否则则是正常的client
    /// </summary>
    public class WorldPlayerStorage
    {
        int _world;
        public WorldPlayerStorage(int world)
        {
            Channels = new Dictionary<int, ChannelPlayerStorage>();
            _world = world;
            WorldData = new ChannelPlayerStorage(world, -999);
        }
        public Dictionary<int, ChannelPlayerStorage> Channels { get; set; }
        public void RelateChannel(int channelId, ChannelPlayerStorage data)
        {
            data.OnChannelAddPlayer += (obj, p) =>
            {
                WorldData.AddPlayer(p);
            };
            Channels[channelId] = data;
        }

        public ChannelPlayerStorage WorldData { get; set; }

        public IPlayer? this[string name] => getCharacterByName(name);
        public IPlayer? getCharacterByName(string name)
        {
            return GetOrAddCharacterByName(name);
        }

        public IPlayer? this[int id] => getCharacterById(id);
        /// <summary>
        /// 所有数据库中存在的玩家都可以获取到
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPlayer? getCharacterById(int id)
        {
            return GetOrAddCharacterById(id);
        }
        public ICollection<IPlayer> getAllCharacters()
        {
            return WorldData.getAllCharacters().Where(x => x.IsOnlined).ToList();
        }

        public int Count()
        {
            return WorldData.Count();
        }

        public void RemovePlayer(int playerId)
        {
            WorldData.RemovePlayer(playerId);
        }

        public void disconnectAll()
        {
            foreach (var item in Channels)
            {
                item.Value.disconnectAll();
            }
        }

        public IPlayer? GetOrAddCharacterById(int id)
        {
            var m = WorldData.getCharacterById(id);
            if (m != null)
                return m;

            m = CharacterManager.GetPlayerById(id);
            if (m != null)
                WorldData.AddPlayer(m);
            return m;
        }

        public List<IPlayer> GetPlayersByIds(IEnumerable<int> idList)
        {
            List<IPlayer> list = new();

            List<int> notFound = new List<int>();
            foreach (var id in idList)
            {
                var m = WorldData.getCharacterById(id);
                if (m != null)
                    list.Add(m);
                else
                    notFound.Add(id);
            }

            list.AddRange(CharacterManager.GetPlayersById(notFound));
            return list;
        }
        public IPlayer? GetOrAddCharacterByName(string name)
        {
            var m = WorldData.getCharacterByName(name);
            if (m != null)
                return m;

            m = CharacterManager.GetPlayerByName(name);
            if (m != null)
                WorldData.AddPlayer(m);
            return m;
        }
        public List<IPlayer> GetPlayersByNames(IEnumerable<string> nameList)
        {
            List<IPlayer> list = new();

            List<string> notFound = new();
            foreach (var id in nameList)
            {
                var m = WorldData.getCharacterByName(id);
                if (m != null)
                    list.Add(m);
                else
                    notFound.Add(id);
            }

            list.AddRange(CharacterManager.GetPlayersByName(notFound));
            return list;
        }
    }
}

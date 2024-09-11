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
        }
        public Dictionary<int, ChannelPlayerStorage> Channels { get; set; }
        public void RelateChannel(int channelId, ChannelPlayerStorage data)
        {
            data.OnChannelAddPlayer += (obj, p) =>
            {
                AllPlayerStorage.AddPlayer(new DataLevel(2, p));
            };
            Channels[channelId] = data;
        }

        public IPlayer? getCharacterByName(string name)
        {
            var m = AllPlayerStorage.GetOrAddCharacterByName(name);

            if (m != null && m.World == _world)
                return m;
            return null;
        }

        public IPlayer? this[int id] => getCharacterById(id);
        /// <summary>
        /// 所有数据库中存在的玩家都可以获取到
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPlayer? getCharacterById(int id)
        {
            var m = AllPlayerStorage.GetOrAddCharacterById(id);

            if (m != null && m.World == _world)
                return m;
            return null;
        }
        /// <summary>
        /// 获取所有在线玩家
        /// </summary>
        /// <returns></returns>
        public ICollection<IPlayer> GetAllOnlinedPlayers()
        {
            return AllPlayerStorage.GetAllOnlinedPlayers().Where(x => x.World == _world).ToList();
        }

        public int Count()
        {
            return GetAllOnlinedPlayers().Count;
        }

        //public void RemovePlayer(int playerId)
        //{
        //    WorldData.RemovePlayer(playerId);
        //}

        public void disconnectAll()
        {
            foreach (var item in Channels)
            {
                item.Value.disconnectAll();
            }
        }

        public List<IPlayer> GetPlayersByIds(IEnumerable<int> idList, int level = 0)
        {
            return AllPlayerStorage.GetPlayersByIds(idList, level).Where(x => x.World == _world).ToList();
        }
        public List<IPlayer> GetPlayersByNames(IEnumerable<string> nameList, int level = 0)
        {
            return AllPlayerStorage.GetPlayersByNames(nameList, level).Where(x => x.World == _world).ToList();
        }
    }
}

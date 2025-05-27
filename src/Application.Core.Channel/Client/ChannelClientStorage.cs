using Application.Core.Game.TheWorld;
using System.Collections.Concurrent;

namespace Application.Core.Client
{
    public class ChannelClientStorage
    {
        public ConcurrentDictionary<int, ChannelClient> _dataSource;
        public ChannelClientStorage(IWorldChannel worldChannel)
        {
            _dataSource = new();
        }

        public void RemoveClient(int accId)
        {
            _dataSource.TryRemove(accId, out _);
        }

        public bool Register(int accId, ChannelClient client) => _dataSource.TryAdd(accId, client);
    }
}

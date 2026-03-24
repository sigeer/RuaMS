using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class BuffManager
    {
        ConcurrentDictionary<int, SyncProto.PlayerBuffDto> _datasource;
        public BuffManager()
        {
            _datasource = new();
        }

        public void SaveBuff(int v, SyncProto.PlayerBuffDto data)
        {
            _datasource[v] = data;
        }

        public SyncProto.PlayerBuffDto Get(int playerId)
        {
            if (_datasource.TryRemove(playerId, out var d))
                return d;
            return new();
        }
    }
}

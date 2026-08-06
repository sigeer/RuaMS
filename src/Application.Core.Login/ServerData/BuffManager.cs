using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class BuffManager
    {
        ConcurrentDictionary<int, ProtoModel.PlayerBuffProto> _datasource;
        public BuffManager()
        {
            _datasource = new();
        }

        public void SaveBuff(int v, ProtoModel.PlayerBuffProto data)
        {
            _datasource[v] = data;
        }

        public ProtoModel.PlayerBuffProto Get(int playerId)
        {
            if (_datasource.TryRemove(playerId, out var d))
                return d;
            return new();
        }
    }
}

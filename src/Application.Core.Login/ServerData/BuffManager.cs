using Application.Core.Login.Models;
using AutoMapper;

namespace Application.Core.Login.Datas
{
    public class BuffManager
    {
        Dictionary<int, PlayerBuffSaveModel> _datasource;

        readonly IMapper _mapper;
        readonly MasterServer _server;

        public BuffManager(IMapper mapper, MasterServer server)
        {
            _datasource = new();
            _mapper = mapper;
            _server = server;
        }

        public void SaveBuff(int v, SyncProto.PlayerBuffDto data)
        {
            _datasource[v] = _mapper.Map<PlayerBuffSaveModel>(data);
        }

        public SyncProto.PlayerBuffDto Get(int playerId)
        {
            if (_datasource.Remove(playerId, out var d))
                return _mapper.Map<SyncProto.PlayerBuffDto>(d);
            return new();
        }
    }
}

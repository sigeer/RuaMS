using Application.Shared.Dto;

namespace Application.Core.Login.Datas
{
    public class PlayerBuffStorage
    {
        Dictionary<int, PlayerBuffSaveDto> _datasource;

        public PlayerBuffStorage()
        {
            _datasource = new Dictionary<int, PlayerBuffSaveDto>();
        }

        public void Save(int playerId, PlayerBuffSaveDto data)
        {
            _datasource[playerId] = data;
        }

        public PlayerBuffSaveDto Get(int playerId)
        {
            if (_datasource.Remove(playerId, out var d))
                return d;
            return new();
        }
    }
}

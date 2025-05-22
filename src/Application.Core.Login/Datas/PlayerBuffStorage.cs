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

        }

        public PlayerBuffSaveDto Get(int playerId)
        {
            return new PlayerBuffSaveDto();
        }
    }
}

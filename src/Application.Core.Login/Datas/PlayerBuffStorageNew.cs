using Application.Shared.Dto;

namespace Application.Core.Login.Datas
{
    public class PlayerBuffStorageNew
    {
        Dictionary<int, PlayerBuffSaveDto> _datasource;

        public PlayerBuffStorageNew()
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

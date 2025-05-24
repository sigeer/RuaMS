using Application.Core.Login.Datas;
using Application.Shared.Dto;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public PlayerBuffStorage BuffStorage { get; }

        public void SaveBuff(int v, PlayerBuffSaveDto playerBuffSaveDto)
        {
            BuffStorage.Save(v, playerBuffSaveDto);
        }

        public PlayerBuffSaveDto GetBuff(int v) => BuffStorage.Get(v);
    }
}

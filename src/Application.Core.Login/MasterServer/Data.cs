using Application.Core.Login.Datas;
using Application.Shared.Dto;
using net.server;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public PlayerBuffStorageNew BuffStorageNew { get; }

        public void SaveBuff(int v, PlayerBuffSaveDto playerBuffSaveDto)
        {
            BuffStorageNew.Save(v, playerBuffSaveDto);
        }

        public PlayerBuffSaveDto GetBuff(int v) => BuffStorageNew.Get(v);
    }
}

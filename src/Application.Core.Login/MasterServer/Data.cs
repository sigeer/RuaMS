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

        public Config.WorldConfig GetWorldConfig()
        {
            return new Config.WorldConfig
            {
                BossDropRate = BossDropRate,
                DropRate = DropRate,
                ExpRate = ExpRate,
                FishingRate = FishingRate,
                MesoRate = MesoRate,
                MobRate = MobRate,
                QuestRate = QuestRate,
                ServerMessage = ServerMessage,
                TravelRate = TravelRate
            };
        }
    }
}

using Application.Shared.Message;
using XmlWzReader;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
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

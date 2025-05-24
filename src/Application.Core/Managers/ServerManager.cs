using Application.Core.EF.Entities.SystemBase;
using Microsoft.EntityFrameworkCore;
using net.server;

namespace Application.Core.Managers
{
    public class ServerManager
    {

        public static WorldConfigEntity? GetWorld(int worldId)
        {
            return null;
        }

        public static string? GetWorldName(int worldId)
        {
            return "";
        }

        public static void ApplyWorldServer(WorldConfigEntity worldConfig)
        {
            var srv = Server.getInstance();
            if (srv.RunningWorlds.TryGetValue(worldConfig.Id, out var runningWorldSrv))
            {
                // 更新世界设置
                //runningWorldSrv.Name = worldConfig.Name;
                //runningWorldSrv.setExpRate(worldConfig.ExpRate);
                //runningWorldSrv.setMesoRate(worldConfig.MesoRate);
                //runningWorldSrv.setDropRate(worldConfig.DropRate);
                //runningWorldSrv.BossDropRate = worldConfig.BossDropRate;
                //runningWorldSrv.FishingRate = worldConfig.FishingRate;
                //runningWorldSrv.TravelRate = worldConfig.TravelRate;
                //runningWorldSrv.QuestRate = worldConfig.QuestRate;

                //runningWorldSrv.EventMessage = worldConfig.EventMessage;
                //runningWorldSrv.ServerMessage = worldConfig.ServerMessage;
                //runningWorldSrv.WhyAmIRecommended = worldConfig.RecommendMessage;

            }
            else
            {
                srv.AddWorld(worldConfig);
            }

        }

        public static void ApplyWorldServer()
        {


        }
    }
}

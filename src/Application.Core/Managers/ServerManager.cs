using Application.Core.EF.Entities.SystemBase;
using Microsoft.EntityFrameworkCore;
using net.server;

namespace Application.Core.Managers
{
    public class ServerManager
    {
        public static List<WorldConfigEntity> LoadAllWorld()
        {
            using var dbContext = new DBContext();
            return dbContext.WorldConfigs.AsNoTracking().Take(1).ToList();
        }

        public static WorldConfigEntity? GetWorld(int worldId)
        {
            return LoadAllWorld().FirstOrDefault(x => x.Id == worldId);
        }

        public static string? GetWorldName(int worldId)
        {
            var srv = Server.getInstance();
            if (srv.RunningWorlds.TryGetValue(worldId, out var runningWorldSrv))
                return runningWorldSrv.Name;

            return LoadAllWorld().FirstOrDefault(x => x.Id == worldId)?.Name;
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
            var worlds = LoadAllWorld();

            foreach (var worldConfig in worlds)
            {
                ApplyWorldServer(worldConfig);
            }

        }
    }
}

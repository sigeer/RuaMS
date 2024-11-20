using Application.Core.EF.Entities.SystemBase;
using Microsoft.EntityFrameworkCore;
using net.server;

namespace Application.Core.Managers
{
    public class ServerManager
    {
        public static List<WorldConfigEntity> AllWorldCache { get; set; } = new List<WorldConfigEntity>();
        public static List<WorldConfigEntity> LoadAllWorld()
        {
            if (AllWorldCache.Count == 0)
            {
                using var dbContext = new DBContext();
                AllWorldCache = dbContext.WorldConfigs.AsNoTracking().ToList();
            }
            return AllWorldCache;
        }

        public static WorldConfigEntity? GetWorld(int worldId)
        {
            return LoadAllWorld().FirstOrDefault(x => x.Id == worldId);
        }


        public static void ApplyWorldServer()
        {
            var worlds = LoadAllWorld();

            var srv = Server.getInstance();
            foreach (var worldConfig in worlds)
            {
                if (srv.RunningWorlds.ContainsKey(worldConfig.Id))
                {
                    if (!worldConfig.CanDeploy)
                        srv.RemoveWorld(worldConfig.Id);
                    else
                    {
                        // 更新世界设置
                    }
                }
                else
                {
                    srv.AddWorld(worldConfig);
                }
            }

        }
    }
}

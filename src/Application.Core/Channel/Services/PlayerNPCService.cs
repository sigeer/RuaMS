using Application.Core.Game.Maps;

namespace Application.Core.Channel.Services
{
    public interface IPlayerNPCService
    {
        public void LoadPlayerNpc(IMap map);
        bool CanSpawn(IMap map, string targetName);
        void SpawnPlayerNPCByHonor(Player chr);
        void SpawnPlayerNPCHere(int mapId, Point position, Player chr);
    }

    public class DefaultPlayerNPCService : IPlayerNPCService
    {
        public bool CanSpawn(IMap map, string targetName)
        {
            return false;
        }

        public void LoadPlayerNpc(IMap map)
        {
            
        }

        public void SpawnPlayerNPCByHonor(Player chr)
        {

        }

        public void SpawnPlayerNPCHere(int mapId, Point position, Player chr)
        {

        }
    }
}

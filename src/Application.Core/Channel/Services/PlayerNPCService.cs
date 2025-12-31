using Application.Core.Game.Maps;

namespace Application.Core.Channel.Services
{
    public interface IPlayerNPCService
    {
        public void LoadPlayerNpc(IMap map);
        bool CanSpawn(IMap map, string targetName);
        void SpawnPlayerNPCByHonor(IPlayer chr);
        void SpawnPlayerNPCHere(int mapId, Point position, IPlayer chr);
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

        public void SpawnPlayerNPCByHonor(IPlayer chr)
        {

        }

        public void SpawnPlayerNPCHere(int mapId, Point position, IPlayer chr)
        {

        }
    }
}

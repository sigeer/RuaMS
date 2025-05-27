using Application.Core.Game.Players.PlayerProps;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerTrockLocation PlayerTrockLocation { get; set; }
        public int[] getTrockMaps()
        {
            return PlayerTrockLocation.GetTrockMaps();
        }

        public int[] getVipTrockMaps()
        {
            return PlayerTrockLocation.GetVipTrockMaps();
        }


        public void deleteFromTrocks(int map)
        {
            PlayerTrockLocation.Delete(map);
        }

        public void addTrockMap()
        {
            PlayerTrockLocation.AddTrockMap(getMapId());
        }

        public void deleteFromVipTrocks(int map)
        {
            PlayerTrockLocation.DeleteVip(map);
        }

        public void addVipTrockMap()
        {
            PlayerTrockLocation.AddVipTrockMap(getMapId());
        }

    }
}

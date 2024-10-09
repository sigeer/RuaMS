using constants.id;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public List<int> getTrockMaps()
        {
            return TrockMaps;
        }

        public List<int> getVipTrockMaps()
        {
            return VipTrockMaps;
        }

        public int getTrockSize()
        {
            int ret = TrockMaps.IndexOf(MapId.NONE);
            if (ret == -1)
            {
                ret = 5;
            }

            return ret;
        }

        public void deleteFromTrocks(int map)
        {
            TrockMaps.Remove(map);
            while (TrockMaps.Count < 10)
            {
                TrockMaps.Add(MapId.NONE);
            }
        }

        public void addTrockMap()
        {
            int index = TrockMaps.IndexOf(MapId.NONE);
            if (index != -1)
            {
                TrockMaps[index] = getMapId();
            }
        }

        public bool isTrockMap(int id)
        {
            int index = TrockMaps.IndexOf(id);
            return index != -1;
        }

        public int getVipTrockSize()
        {
            int ret = VipTrockMaps.IndexOf(MapId.NONE);

            if (ret == -1)
            {
                ret = 10;
            }

            return ret;
        }

        public void deleteFromVipTrocks(int map)
        {
            VipTrockMaps.Remove(map);
            while (VipTrockMaps.Count < 10)
            {
                VipTrockMaps.Add(MapId.NONE);
            }
        }

        public void addVipTrockMap()
        {
            int index = VipTrockMaps.IndexOf(MapId.NONE);
            if (index != -1)
            {
                VipTrockMaps[index] = getMapId();
            }
        }

        public bool isVipTrockMap(int id)
        {
            int index = VipTrockMaps.IndexOf(id);
            return index != -1;
        }

    }
}

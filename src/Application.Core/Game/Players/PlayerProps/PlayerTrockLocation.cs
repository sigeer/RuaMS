using constants.id;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerTrockLocation : PlayerPropBase
    {
        int[] _dataSouce;
        int[] _vipDataSouce;
        int _size;
        int _vipSize;
        public PlayerTrockLocation(IPlayer owner, int size, int vipSize) : base(owner)
        {
            _size = size;
            _vipSize = vipSize;

            _dataSouce = Enumerable.Repeat(MapId.NONE, _size).ToArray();
            _vipDataSouce = Enumerable.Repeat(MapId.NONE, _vipSize).ToArray();
        }


        public override void LoadData(DBContext dbContext)
        {
            _dataSouce = Enumerable.Repeat(MapId.NONE, _size).ToArray();
            _vipDataSouce = Enumerable.Repeat(MapId.NONE, _vipSize).ToArray();

            var trockLocList = dbContext.Trocklocations.Where(x => x.Characterid == Owner.Id).Select(x => new { x.Vip, x.Mapid }).Take(15).ToList();

            byte vip = 0;
            byte reg = 0;

            foreach (var item in trockLocList)
            {
                if (item.Vip == 1)
                {
                    _vipDataSouce[vip] = item.Mapid;
                    vip++;
                }
                else
                {
                    _dataSouce[reg] = item.Mapid;
                    reg++;
                }
            }
        }

        public override void SaveData(DBContext dbContext)
        {
            dbContext.Trocklocations.Where(x => x.Characterid == Owner.Id).ExecuteDelete();
            for (int i = 0; i < _size; i++)
            {
                if (_dataSouce[i] != MapId.NONE)
                {
                    dbContext.Trocklocations.Add(new Trocklocation(Owner.Id, _dataSouce[i], 0));
                }
            }

            for (int i = 0; i < _vipSize; i++)
            {
                if (_vipDataSouce[i] != MapId.NONE)
                {
                    dbContext.Trocklocations.Add(new Trocklocation(Owner.Id, _vipDataSouce[i], 1));
                }
            }
            dbContext.SaveChanges();
        }

        public int[] GetTrockMaps()
        {
            return _dataSouce;
        }

        public int[] GetVipTrockMaps()
        {
            return _vipDataSouce;
        }

        public void AddTrockMap(int id)
        {
            int index = Array.IndexOf(_dataSouce, MapId.NONE);
            if (index != -1)
            {
                _dataSouce[index] = id;
            }
        }

        public void AddVipTrockMap(int id)
        {
            int index = Array.IndexOf(_vipDataSouce, MapId.NONE);
            if (index != -1)
            {
                _vipDataSouce[index] = id;
            }
        }

        public void Delete(int id)
        {
            int index = Array.IndexOf(_dataSouce, id);
            if (index != -1)
                _dataSouce[index] = MapId.NONE;

            Array.Sort(_dataSouce);
        }

        public void DeleteVip(int id)
        {
            int index = Array.IndexOf(_vipDataSouce, id);
            if (index != -1)
                _vipDataSouce[index] = MapId.NONE;

            Array.Sort(_vipDataSouce);
        }
    }
}

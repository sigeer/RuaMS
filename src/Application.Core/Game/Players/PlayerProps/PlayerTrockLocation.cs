using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    /// <summary>
    /// 传送石
    /// </summary>
    public class PlayerTrockLocation : PlayerPropBase<Dto.TrockLocationDto>
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

        public override void LoadData(RepeatedField<Dto.TrockLocationDto> trockLocList)
        {
            _dataSouce = Enumerable.Repeat(MapId.NONE, _size).ToArray();
            _vipDataSouce = Enumerable.Repeat(MapId.NONE, _vipSize).ToArray();

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

        public override Dto.TrockLocationDto[] ToDto()
        {
            return _dataSouce.Select(x => new Dto.TrockLocationDto() { Mapid = x, Vip = 0 })
                .Concat(_vipDataSouce.Select(x => new Dto.TrockLocationDto() { Mapid = x, Vip = 1 })).ToArray();
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

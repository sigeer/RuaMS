using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerSavedLocation : PlayerPropBase<Dto.SavedLocationDto>
    {
        public SavedLocationType[] AllType { get; set; }
        public PlayerSavedLocation(Player owner) : base(owner)
        {
            AllType = Enum.GetValues<SavedLocationType>();
            _dataSource = new SavedLocation[AllType.Length];
        }

        SavedLocation?[] _dataSource;
        public override void LoadData(RepeatedField<Dto.SavedLocationDto> savedLocFromDB)
        {
            foreach (var item in savedLocFromDB)
            {
                _dataSource[(int)Enum.Parse<SavedLocationType>(item.Locationtype)] = new SavedLocation(item.Map, item.Portal);
            }
        }

        public override Dto.SavedLocationDto[] ToDto()
        {
            return _dataSource.Where(x => x != null).Select((x, idx) => new Dto.SavedLocationDto
            {
                Locationtype = ((SavedLocationType)idx).ToString(),
                Map = x.getMapId(),
                Portal = x.getPortal(),
            }).ToArray();
        }
        public void FillData(SavedLocation data)
        {
            for (int i = 0; i < _dataSource.Length; i++)
            {
                if (_dataSource[i] == null)
                {
                    _dataSource[i] = data;
                }
            }
        }

        private void AddOrUpdate(int index, SavedLocation? value)
        {
            _dataSource[index] = value;
        }

        public void AddOrUpdate(SavedLocationType type, SavedLocation? value)
        {
            AddOrUpdate(GetKey(type), value);
        }

        public void AddOrUpdate(string typeName, SavedLocation? value)
        {
            AddOrUpdate(GetKey(typeName), value);
        }
        private int GetKey(string typeName) => GetKey(SavedLocationTypeUtils.fromString(typeName));
        private int GetKey(SavedLocationType type) => (int)type;

        public SavedLocation? GetData(int key) => _dataSource.ElementAtOrDefault(key);
        public SavedLocation? GetData(string typeName) => _dataSource.ElementAtOrDefault(GetKey(typeName));
        public SavedLocation? GetData(SavedLocationType type) => _dataSource.ElementAtOrDefault(GetKey(type));
    }
}

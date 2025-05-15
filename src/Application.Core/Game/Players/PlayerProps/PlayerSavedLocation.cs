using Application.Shared.Characters;
using Microsoft.EntityFrameworkCore;
using server.maps;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerSavedLocation : PlayerPropBase
    {
        public SavedLocationType[] AllType { get; set; }
        public PlayerSavedLocation(IPlayer owner) : base(owner)
        {
            AllType = Enum.GetValues<SavedLocationType>();
            _dataSource = new SavedLocation[AllType.Length];
        }

        SavedLocation?[] _dataSource;
        public void LoadData(SavedLocationDto[] savedLocFromDB)
        {
            foreach (var item in savedLocFromDB)
            {
                _dataSource[(int)Enum.Parse<SavedLocationType>(item.Type)] = new SavedLocation(item.MapId, item.Portal);
            }

        }
        public override void LoadData(DBContext dbContext)
        {
            var savedLocFromDB = dbContext.Savedlocations.Where(x => x.Characterid == Owner.Id).Select(x => new { x.Locationtype, x.Map, x.Portal }).ToList();
            foreach (var item in savedLocFromDB)
            {
                _dataSource[(int)Enum.Parse<SavedLocationType>(item.Locationtype)] = new SavedLocation(item.Map, item.Portal);
            }

        }

        public override void SaveData(DBContext dbContext)
        {
            dbContext.Savedlocations.Where(x => x.Characterid == Owner.Id).ExecuteDelete();
            dbContext.Savedlocations.AddRange(
                AllType
                .Where(x => _dataSource[(int)x] != null)
                .Select(x => new SavedLocationEntity(Owner.Id, _dataSource[(int)x]!, x))
                );
            dbContext.SaveChanges();
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

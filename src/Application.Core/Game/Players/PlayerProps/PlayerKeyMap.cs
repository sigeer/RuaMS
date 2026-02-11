using Application.Shared.KeyMaps;
using client.keybind;
using constants.game;
using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerKeyMap : PlayerPropBase<Dto.KeyMapDto>
    {
        private Dictionary<int, KeyBinding> _dataSource;
        public PlayerKeyMap(Player owner) : base(owner)
        {
            _dataSource = GameConstants.GetDefaultKeyMapping();
        }

        public override void LoadData(RepeatedField<Dto.KeyMapDto> keyMapFromDB)
        {
            _dataSource.Clear();

            foreach (var item in keyMapFromDB)
            {
                _dataSource[item.Key] = new KeyBinding(item.Type, item.Action);
            }
        }

        public override Dto.KeyMapDto[] ToDto()
        {
            return _dataSource.Select(x => new Dto.KeyMapDto
            {
                Key = x.Key,
                Action = x.Value.getAction(),
                Type = x.Value.getType()
            }).ToArray();
        }
        public void AddOrUpdate(int key, KeyBinding value)
        {
            _dataSource[key] = value;
        }

        public void Remove(int key)
        {
            _dataSource.Remove(key);
        }

        public Dictionary<int, KeyBinding> GetDataSource()
        {
            return _dataSource;
        }

        public KeyBinding? GetData(int key) => _dataSource.GetValueOrDefault(key);
        public KeyBinding? GetData(KeyCode key) => GetData((int)key);
    }
}

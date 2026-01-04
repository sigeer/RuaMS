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
            _dataSource = new Dictionary<int, KeyBinding>();

            var selectedKey = GameConstants.getCustomKey(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            var selectedType = GameConstants.getCustomType(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            var selectedAction = GameConstants.getCustomAction(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            for (int i = 0; i < selectedKey.Length; i++)
            {
                _dataSource.AddOrUpdate(selectedKey[i], new KeyBinding(selectedType[i], selectedAction[i]));
            }
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

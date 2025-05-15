using Application.Shared.Characters;
using Application.Shared.KeyMaps;
using client.keybind;
using constants.game;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerKeyMap : PlayerPropBase
    {
        private Dictionary<int, KeyBinding> _dataSource;
        public PlayerKeyMap(IPlayer owner) : base(owner)
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

        public void LoadData(KeyMapDto[] keyMapFromDB)
        {
            _dataSource.Clear();

            foreach (var item in keyMapFromDB)
            {
                _dataSource.AddOrUpdate(item.Key, new KeyBinding(item.Type, item.Action));
            }
        }

        public override void LoadData(DBContext dbContext)
        {
            _dataSource.Clear();
            var keyMapFromDB = dbContext.Keymaps.Where(x => x.Characterid == Owner.Id).Select(x => new { x.Key, x.Type, x.Action }).ToList();
            foreach (var item in keyMapFromDB)
            {
                _dataSource.AddOrUpdate(item.Key, new KeyBinding(item.Type, item.Action));
            }
        }

        public override void SaveData(DBContext dbContext)
        {
            dbContext.Keymaps.Where(x => x.Characterid == Owner.Id).ExecuteDelete();
            dbContext.Keymaps.AddRange(_dataSource.Select(x => new Keymap() { Characterid = Owner.Id, Key = x.Key, Type = x.Value.getType(), Action = x.Value.getAction() }));
            dbContext.SaveChanges();
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

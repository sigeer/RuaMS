namespace Application.Core.Channel.ResourceTransaction.Handlers
{
    internal class PlayerCostItemIdHandler : IResourceHandler
    {
        readonly IPlayer _player;
        ItemQuantity? _freezedItem;

        readonly ItemQuantity _costItems;
        public PlayerCostItemIdHandler(IPlayer chr, ItemQuantity data)
        {
            _player = chr;
            _costItems = data;
        }
        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.RemoveItemById(ItemConstants.getInventoryType(_costItems.ItemId), _costItems.ItemId, (short)_costItems.Quantity))
                {
                    _freezedItem = _costItems;
                    return true;
                }
                return false;
            }
        }

        public void Commit()
        {
            lock (_player.ResourceLock)
            {
                _freezedItem = null;
            }
        }

        public void Rollback()
        {
            lock (_player.ResourceLock)
            {
                if (_freezedItem != null)
                {
                    _player.GainItem(_freezedItem.ItemId, (short)_freezedItem.Quantity, false, false);
                    _freezedItem = null;
                }
            }
        }
    }

    internal class PlayerGainItemIdHandler : IResourceHandler
    {
        readonly IPlayer _player;
        ItemQuantity? _freezedItem;

        readonly ItemQuantity _deltaItem;
        public PlayerGainItemIdHandler(IPlayer chr, ItemQuantity data)
        {
            _player = chr;
            _deltaItem = data;
        }
        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                var gainedItem = _player.GainItem(_deltaItem.ItemId, (short)_deltaItem.Quantity, false, false);
                if (gainedItem != null)
                {
                    _freezedItem = _deltaItem;
                    return true;
                }
                return false;
            }
        }

        public void Commit()
        {
            lock (_player.ResourceLock)
            {
                _freezedItem = null;
            }
        }

        public void Rollback()
        {
            lock (_player.ResourceLock)
            {
                if (_freezedItem != null)
                {
                    _player.RemoveItemById(ItemConstants.getInventoryType(_freezedItem.ItemId), _freezedItem.ItemId, (short)_freezedItem.Quantity);
                    _freezedItem = null;
                }
            }
        }
    }
}

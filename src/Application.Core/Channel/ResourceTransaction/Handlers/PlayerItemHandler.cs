using Application.Core.model;
using client.inventory;
using client.inventory.manipulator;

namespace Application.Core.Channel.ResourceTransaction.Handlers
{
    internal class PlayerCostItemHandler : IResourceHandler
    {
        readonly IPlayer _player;
        Item? _freezedItem;

        readonly ItemObjectQuantity _costItems;
        public PlayerCostItemHandler(IPlayer chr, ItemObjectQuantity data)
        {
            _player = chr;
            _costItems = data;
        }
        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.RemoveItemBySlot(_costItems.Item.getInventoryType(), _costItems.Item.getPosition(), (short)_costItems.Quantity))
                {
                    _freezedItem = _costItems.Item.copy();
                    _freezedItem.setQuantity((short)_costItems.Quantity);
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
                    InventoryManipulator.addFromDrop(_player.Client, _freezedItem, false);
                    _freezedItem = null;
                }
            }
        }
    }

    internal class PlayerGainItemHandler : IResourceHandler
    {
        readonly IPlayer _player;
        Item? _freezedItem;

        readonly Item _deltaItem;
        public PlayerGainItemHandler(IPlayer chr, Item data)
        {
            _player = chr;
            _deltaItem = data;
        }
        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (InventoryManipulator.addFromDrop(_player.Client, _deltaItem, false))
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
                    _player.RemoveItemBySlot(_freezedItem.getInventoryType(), _freezedItem.getPosition(), _freezedItem.getQuantity());
                    _freezedItem = null;
                }
            }
        }
    }
}

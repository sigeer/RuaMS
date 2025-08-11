using client.inventory;
using client.inventory.manipulator;

namespace Application.Core.Game.Players
{
    public class PlayerBag : IDisposable
    {
        readonly Inventory[] _dataSource;
        private bool disposedValue;
        IPlayer Owner { get; }
        public PlayerBag(IPlayer owner)
        {
            Owner = owner;
            var typeList = EnumCache<InventoryType>.Values;
            _dataSource = new Inventory[typeList.Length];

            for (int i = 0; i < typeList.Length; i++)
            {
                var type = typeList[i];
                byte b = BagConfig.DefaultSize;
                if (type == InventoryType.CASH)
                {
                    b = BagConfig.CashSize;
                }
                _dataSource[i] = new Inventory(owner, type, b);
            }
            _dataSource[InventoryType.CANHOLD.ordinal()] = new InventoryProof(owner);
        }

        public Inventory this[InventoryType type] => this[type.ordinal()];
        public Inventory this[int typeOrdinal] => _dataSource[typeOrdinal];

        public void SetValue(InventoryType type, Inventory inventory)
        {
            _dataSource[type.ordinal()] = inventory;
        }

        public Inventory[] GetValues()
        {
            return _dataSource;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    for (int i = 0; i < _dataSource.Length; i++)
                    {
                        _dataSource[i].dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PlayerBag()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void RemoveFromSlot(InventoryType type, short slot, short quantity, bool fromDrop, bool consume = false)
        {
            Inventory inv = this[type];
            var item = inv.getItem(slot);
            if (item == null)
                return;

            inv.lockInventory();
            try
            {
                if (type == InventoryType.EQUIPPED)
                {
                    Owner.unequippedItem((Equip)item);
                }
                else
                {
                    var petid = item.PetId;
                    if (petid > -1)
                    {
                        int petIdx = Owner.getPetIndex(petid);
                        if (petIdx > -1)
                        {
                            var pet = Owner.getPet(petIdx)!;
                            Owner.unequipPet(pet, true);
                        }
                    }
                }

                bool allowZero = consume && ItemConstants.isRechargeable(item.getItemId());
                inv.removeItem(slot, quantity, allowZero);
                if (type != InventoryType.CANHOLD)
                {
                    InventoryManipulator.AnnounceModifyInventory(Owner.Client, item, fromDrop, allowZero);
                }
            }
            finally
            {
                inv.unlockInventory();
            }
        }
    }
}

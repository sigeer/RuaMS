/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Client.inventory;

namespace client.inventory
{
    public interface IInventory: IEnumerable<Item>, IDisposable
    {
        Player Owner { get; }

        InventoryAdd? AddItem(Item item);
        int countById(int itemId);
        int countNotOwnedById(int itemId);
        Item? findByCashId(long cashId);
        Item? findById(int itemId);
        Item? findByName(string name);
        Item? getItem(short slot);
        InventoryType getType();
        bool HasItem(int itemId);
        InventoryAdd? InsertItem(short position, Item item);
        bool IsChecked();
        bool isEquipInventory();
        bool isExtendableInventory();
        bool isFull();
        bool isFull(int margin);
        bool isFullAfterSomeItems(int margin, int used);
        IList<Item> list();
        List<Item> listById(int itemId);
        void RemoveFromMove(short slot);
        IInventoryOperationCommand? removeItem(short slot, out short actualRemoved, short quantity = 1, bool allowZero = false);
        IInventoryOperationCommand? removeSlot(short slot);
        void SetItemPosition(Item item, short pos);
        void SwapFromMove(short sSlot, short dSlot);
        IInventoryOperationCommand? Take(short slot, short quantity, out Item? item);
    }
}
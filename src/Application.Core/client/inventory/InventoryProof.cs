/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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

namespace client.inventory;

/**
 * @author Ronan
 */
public class InventoryProof : Inventory
{

    public InventoryProof(Player mc) : base(mc, InventoryType.CANHOLD, 0)
    {

    }

    public void cloneContents(Inventory inv)
    {
        lockObj.Enter();
        try
        {
            inventory.Clear();
            this.setSlotLimit(inv.getSlotLimit());

            foreach (Item it in inv.list())
            {
                Item item = new Item(it.getItemId(), it.getPosition(), it.getQuantity());
                inventory.AddOrUpdate(item.getPosition(), item);
            }
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public void flushContents()
    {
        lockObj.Enter();
        try
        {
            inventory.Clear();
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public override short addSlot(Item item)
    {
        if (item == null)
        {
            return -1;
        }

        lockObj.Enter();
        try
        {
            short slotId = getNextFreeSlot();
            if (slotId < 0)
            {
                return -1;
            }
            inventory.AddOrUpdate(slotId, item);

            return slotId;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public override void addSlotFromDB(short slot, Item item)
    {
        lockObj.Enter();
        try
        {
            inventory.AddOrUpdate(slot, item);
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public override void removeSlot(short slot)
    {
        lockObj.Enter();
        try
        {
            inventory.Remove(slot);
        }
        finally
        {
            lockObj.Exit();
        }
    }
}

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


using client.inventory;

namespace server.loot;

/**
 * @author Ronan
 */
public class LootInventory
{
    Dictionary<int, int> items = new(50);

    public LootInventory(IPlayer from)
    {
        foreach (InventoryType values in EnumCache<InventoryType>.Values)
        {

            foreach (Item it in from.getInventory(values).list())
            {
                items.AddOrUpdate(it.getItemId(), items.GetValueOrDefault(it.getItemId()) + it.getQuantity());
            }
        }
    }

    public int hasItem(int itemid, int quantity)
    {
        if (!items.TryGetValue(itemid, out var itemQty))
            return 0;

        return itemQty >= quantity ? 2 : (itemQty > 0 ? 1 : 0);
    }

}

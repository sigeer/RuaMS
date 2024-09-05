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
using constants.inventory;

namespace server;



/**
 * @author RonanLana
 */
public class StorageInventory
{
    private IClient c;
    private Dictionary<short, Item> inventory = new();
    private byte slotLimit;

    public StorageInventory(IClient c, List<Item> toSort)
    {
        this.inventory = new();
        this.slotLimit = (byte)toSort.Count;
        this.c = c;

        foreach (Item item in toSort)
        {
            this.addItem(item);
        }
    }

    private byte getSlotLimit()
    {
        return slotLimit;
    }

    private ICollection<Item> list()
    {
        return inventory.Values.ToList();
    }

    private short addItem(Item item)
    {
        short slotId = getNextFreeSlot();
        if (slotId < 0 || item == null)
        {
            return -1;
        }
        addSlot(slotId, item);
        item.setPosition(slotId);
        return slotId;
    }

    private static bool isEquipOrCash(Item item)
    {
        int type = item.getItemId() / 1000000;
        return type == 1 || type == 5;
    }

    private static bool isSameOwner(Item source, Item target)
    {
        return source.getOwner().Equals(target.getOwner());
    }

    private void move(short sSlot, short dSlot, short slotMax)
    {
        var source = inventory.GetValueOrDefault(sSlot);
        if (source == null)
        {
            return;
        }
        var target = inventory.GetValueOrDefault(dSlot);
        if (target == null)
        {
            source.setPosition(dSlot);
            inventory.AddOrUpdate(dSlot, source);
            inventory.Remove(sSlot);
        }
        else if (target.getItemId() == source.getItemId()
            && !ItemConstants.isRechargeable(source.getItemId())
            && !ItemInformationProvider.getInstance().isPickupRestricted(source.getItemId())
            && isSameOwner(source, target))
        {
            if (isEquipOrCash(source))
            {
                swap(target, source);
            }
            else if (source.getQuantity() + target.getQuantity() > slotMax)
            {
                short rest = (short)((source.getQuantity() + target.getQuantity()) - slotMax);
                source.setQuantity(rest);
                target.setQuantity(slotMax);
            }
            else
            {
                target.setQuantity((short)(source.getQuantity() + target.getQuantity()));
                inventory.Remove(sSlot);
            }
        }
        else
        {
            swap(target, source);
        }
    }

    private void moveItem(short src, short dst)
    {
        if (src < 0 || dst < 0)
        {
            return;
        }
        if (dst > this.getSlotLimit())
        {
            return;
        }

        Item source = this.getItem(src);
        if (source == null)
        {
            return;
        }
        short slotMax = ItemInformationProvider.getInstance().getSlotMax(c, source.getItemId());
        this.move(src, dst, slotMax);
    }

    private void swap(Item source, Item target)
    {
        inventory.Remove(source.getPosition());
        inventory.Remove(target.getPosition());
        short swapPos = source.getPosition();
        source.setPosition(target.getPosition());
        target.setPosition(swapPos);
        inventory.AddOrUpdate(source.getPosition(), source);
        inventory.AddOrUpdate(target.getPosition(), target);
    }

    private Item? getItem(short slot)
    {
        return inventory.GetValueOrDefault(slot);
    }

    private void addSlot(short slot, Item item)
    {
        inventory.AddOrUpdate(slot, item);
    }

    private void removeSlot(short slot)
    {
        inventory.Remove(slot);
    }

    private bool isFull()
    {
        return inventory.Count >= slotLimit;
    }

    private short getNextFreeSlot()
    {
        if (isFull())
        {
            return -1;
        }

        for (short i = 1; i <= slotLimit; i++)
        {
            if (!inventory.ContainsKey(i))
            {
                return i;
            }
        }
        return -1;
    }

    public void mergeItems()
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        Item? srcItem, dstItem;

        for (short dst = 1; dst <= this.getSlotLimit(); dst++)
        {
            dstItem = this.getItem(dst);
            if (dstItem == null)
            {
                continue;
            }

            for (short src = (short)(dst + 1); src <= this.getSlotLimit(); src++)
            {
                srcItem = this.getItem(src);
                if (srcItem == null)
                {
                    continue;
                }

                if (dstItem.getItemId() != srcItem.getItemId())
                {
                    continue;
                }
                if (dstItem.getQuantity() == ii.getSlotMax(c, this.getItem(dst).getItemId()))
                {
                    break;
                }

                moveItem(src, dst);
            }
        }

        bool sorted = false;

        while (!sorted)
        {
            short freeSlot = this.getNextFreeSlot();

            if (freeSlot != -1)
            {
                short itemSlot = -1;
                for (short i = (short)(freeSlot + 1); i <= this.getSlotLimit(); i = (short)(i + 1))
                {
                    if (this.getItem(i) != null)
                    {
                        itemSlot = i;
                        break;
                    }
                }
                if (itemSlot > 0)
                {
                    moveItem(itemSlot, freeSlot);
                }
                else
                {
                    sorted = true;
                }
            }
            else
            {
                sorted = true;
            }
        }
    }

    public List<Item> sortItems()
    {
        List<Item> itemarray = new();

        for (short i = 1; i <= this.getSlotLimit(); i++)
        {
            var item = this.getItem(i);
            if (item != null)
            {
                itemarray.Add(item.copy());
            }
        }

        foreach (Item item in itemarray)
        {
            this.removeSlot(item.getPosition());
        }

        int invTypeCriteria = 1;
        int sortCriteria = (YamlConfig.config.server.USE_ITEM_SORT_BY_NAME == true) ? 2 : 0;
        PairedQuicksort pq = new PairedQuicksort(itemarray, sortCriteria, invTypeCriteria);

        inventory.Clear();
        return itemarray;
    }
}

class PairedQuicksort
{
    private int i = 0;
    private int j = 0;
    private List<int> intersect;
    ItemInformationProvider ii = ItemInformationProvider.getInstance();

    private void PartitionByItemId(int Esq, int Dir, List<Item> A)
    {
        Item x, w;

        i = Esq;
        j = Dir;

        x = A.get((i + j) / 2);
        do
        {
            while (x.getItemId() > A.get(i).getItemId())
            {
                i++;
            }
            while (x.getItemId() < A.get(j).getItemId())
            {
                j--;
            }

            if (i <= j)
            {
                w = A.get(i);
                A.set(i, A.get(j));
                A.set(j, w);

                i++;
                j--;
            }
        } while (i <= j);
    }

    private void PartitionByName(int Esq, int Dir, List<Item> A)
    {
        Item x, w;

        i = Esq;
        j = Dir;

        x = A.get((i + j) / 2);
        do
        {
            while (ii.getName(x.getItemId()).CompareTo(ii.getName(A.get(i).getItemId())) > 0)
            {
                i++;
            }
            while (ii.getName(x.getItemId()).CompareTo(ii.getName(A.get(j).getItemId())) < 0)
            {
                j--;
            }

            if (i <= j)
            {
                w = A.get(i);
                A.set(i, A.get(j));
                A.set(j, w);

                i++;
                j--;
            }
        } while (i <= j);
    }

    private void PartitionByQuantity(int Esq, int Dir, List<Item> A)
    {
        Item x, w;

        i = Esq;
        j = Dir;

        x = A.get((i + j) / 2);
        do
        {
            while (x.getQuantity() > A.get(i).getQuantity())
            {
                i++;
            }
            while (x.getQuantity() < A.get(j).getQuantity())
            {
                j--;
            }

            if (i <= j)
            {
                w = A.get(i);
                A.set(i, A.get(j));
                A.set(j, w);

                i++;
                j--;
            }
        } while (i <= j);
    }

    private void PartitionByLevel(int Esq, int Dir, List<Item> A)
    {
        Equip x, w, eqpI, eqpJ;

        i = Esq;
        j = Dir;

        x = (Equip)(A.get((i + j) / 2));

        do
        {
            eqpI = (Equip)A.get(i);
            eqpJ = (Equip)A.get(j);

            while (x.getLevel() > eqpI.getLevel())
            {
                i++;
            }
            while (x.getLevel() < eqpJ.getLevel())
            {
                j--;
            }

            if (i <= j)
            {
                w = (Equip)A.get(i);
                A.set(i, A.get(j));
                A.set(j, w);

                i++;
                j--;
            }
        } while (i <= j);
    }

    void MapleQuicksort(int Esq, int Dir, List<Item> A, int sort)
    {
        switch (sort)
        {
            case 3:
                PartitionByLevel(Esq, Dir, A);
                break;

            case 2:
                PartitionByName(Esq, Dir, A);
                break;

            case 1:
                PartitionByQuantity(Esq, Dir, A);
                break;

            default:
                PartitionByItemId(Esq, Dir, A);
                break;
        }


        if (Esq < j)
        {
            MapleQuicksort(Esq, j, A, sort);
        }
        if (i < Dir)
        {
            MapleQuicksort(i, Dir, A, sort);
        }
    }

    public PairedQuicksort(List<Item> A, int primarySort, int secondarySort)
    {
        intersect = new();

        if (A.Count > 0)
        {
            MapleQuicksort(0, A.Count - 1, A, primarySort);
        }

        intersect.Add(0);
        for (int ind = 1; ind < A.Count; ind++)
        {
            if (A.get(ind - 1).getItemId() != A.get(ind).getItemId())
            {
                intersect.Add(ind);
            }
        }
        intersect.Add(A.Count);

        for (int ind = 0; ind < intersect.Count - 1; ind++)
        {
            if (intersect.get(ind + 1) > intersect.get(ind))
            {
                MapleQuicksort(intersect.get(ind), intersect.get(ind + 1) - 1, A, secondarySort);
            }
        }
    }
}
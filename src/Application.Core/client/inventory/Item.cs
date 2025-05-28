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


using Application.Core.Game.Items;
using Application.Core.Managers;
using client.inventory.manipulator;
using server;

namespace client.inventory;

public class Item : IComparable<Item>, IItemProp
{
    private static AtomicInteger runningCashId = new AtomicInteger(777000000);  // pets & rings shares cashid values
    protected ILogger log;
    private int id;
    private int cashId;
    private int sn;
    private short position;
    private short quantity;
    public int PetId { get; set; } = -1;
    private Pet? pet = null;
    private string owner = "";
    protected List<string> itemLog;
    private short flag;
    private long expiration = -1;
    private string giftFrom = "";

    public bool NeedCheckSpace => !ItemId.isNxCard(getItemId())
                                && !ItemInformationProvider.getInstance().isConsumeOnPickup(getItemId());

    public Item(int id, short position, short quantity)
    {
        log = LogFactory.GetLogger(LogType.Item);
        this.id = id;
        this.position = position;
        this.quantity = quantity;
        this.itemLog = new();
        this.flag = 0;
    }

    public Item(int id, short position, short quantity, int petid) : this(id, position, quantity)
    {
        if (petid > -1)
        {
            // issue with null "pet" having petid > -1 found thanks to MedicOP
            this.pet = ItemManager.loadFromDb(id, position, petid);
            if (this.pet == null)
            {
                petid = -1;
            }
        }
        this.PetId = petid;
    }

    public virtual Item copy()
    {
        Item ret = new Item(id, position, quantity, PetId);
        ret.flag = flag;
        ret.owner = owner;
        ret.expiration = expiration;
        ret.itemLog = new(itemLog);
        return ret;
    }

    public void setPosition(short position)
    {
        this.position = position;
        if (this.pet != null)
        {
            this.pet.setPosition(position);
        }
    }

    public virtual void setQuantity(short quantity)
    {
        this.quantity = quantity;
    }

    public void SetPet(Pet? petObj)
    {
        this.pet = petObj;
        this.PetId = petObj?.PetId ?? -1;
    }

    public int getItemId()
    {
        return id;
    }

    public int getCashId()
    {
        if (cashId == 0)
        {
            cashId = runningCashId.getAndIncrement();
        }
        return cashId;
    }

    public short getPosition()
    {
        return position;
    }

    public short getQuantity()
    {
        return quantity;
    }

    public InventoryType getInventoryType()
    {
        return ItemConstants.getInventoryType(id);
    }

    public virtual sbyte getItemType()
    { // 1: equip, 3: pet, 2: other
        if (getPetId() > -1)
        {
            return 3;
        }
        return 2;
    }

    public string getOwner()
    {
        return owner;
    }

    public void setOwner(string owner)
    {
        this.owner = owner;
    }

    public int getPetId()
    {
        return PetId;
    }

    public int CompareTo(Item? other)
    {
        if (other == null)
            return 1;

        if (this.id < other.getItemId())
        {
            return -1;
        }
        else if (this.id > other.getItemId())
        {
            return 1;
        }
        return 0;
    }

    public override string ToString()
    {
        return "Item: " + id + " quantity: " + quantity;
    }

    public List<string> getItemLog()
    {
        return itemLog.ToList();
    }

    public virtual short getFlag()
    {
        return flag;
    }

    public virtual void setFlag(short b)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (ii.isAccountRestricted(id))
        {
            b |= ItemConstants.ACCOUNT_SHARING; // thanks Shinigami15 for noticing ACCOUNT_SHARING flag not being applied properly to items server-side
        }

        this.flag = b;
    }

    public long getExpiration()
    {
        return expiration;
    }

    public void setExpiration(long expire)
    {
        this.expiration = !ItemConstants.isPermanentItem(id) ? expire : ItemConstants.isPet(id) ? long.MaxValue : -1;
    }

    public void setExpiration(DateTimeOffset expire)
    {
        this.expiration = !ItemConstants.isPermanentItem(id) ? expire.ToUnixTimeMilliseconds() : ItemConstants.isPet(id) ? long.MaxValue : -1;
    }

    public int getSN()
    {
        return sn;
    }

    public void setSN(int sn)
    {
        this.sn = sn;
    }

    public string getGiftFrom()
    {
        return giftFrom;
    }

    public void setGiftFrom(string giftFrom)
    {
        this.giftFrom = giftFrom;
    }

    public Pet? getPet()
    {
        return pet;
    }

    public bool isUntradeable()
    {
        return ((this.getFlag() & ItemConstants.UNTRADEABLE) == ItemConstants.UNTRADEABLE)
            || (ItemInformationProvider.getInstance().isDropRestricted(this.getItemId()) && !KarmaManipulator.hasKarmaFlag(this));
    }
}

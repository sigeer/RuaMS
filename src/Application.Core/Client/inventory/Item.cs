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


using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Templates;
using Application.Templates.Item.Consume;
using client.inventory.manipulator;
using System.Runtime.ConstrainedExecution;

namespace client.inventory;

public class Item : IComparable<Item>
{
    protected ILogger log;
    protected int id;
    protected int sn;
    protected short position;
    protected short quantity;

    protected string owner = "";
    protected List<string> itemLog;
    protected short flag;
    public ItemFlag Flag { get; set; }
    protected long expiration = -1;
    protected string giftFrom = "";
    /// <summary>
    /// 封印过期时间
    /// </summary>
    public long LockExpiration { get; set; }

    public long UniqueId { get; private set; }
    public string? Properties { get; set; }
    public bool NeedCheckSpace => !ItemId.isNxCard(getItemId())
                                && !ItemInformationProvider.getInstance().isConsumeOnPickup(getItemId());
    /// <summary>
    /// 不可叠放
    /// </summary>
    public bool CannotStack => SourceTemplate.SlotMax <= 1 || ItemConstants.isRechargeable(getItemId());

    AbstractItemTemplate? _sourceTemplate;
    public virtual AbstractItemTemplate SourceTemplate
    {
        get
        {
            if (_sourceTemplate == null)
                _sourceTemplate = ItemInformationProvider.getInstance().GetItemTemplate(id) ?? throw new BusinessResException($"ItemId = {id}");
            return _sourceTemplate;
        }
    }
    /// <summary>
    /// MapItem 时为null
    /// </summary>
    public IItemStore? Store { get; set; }
    public Item(int id, short position, short quantity, long uniqueId)
    {
        log = LogFactory.GetLogger(LogType.Item);
        this.id = id;
        this.position = position;
        this.quantity = quantity;
        this.itemLog = new();

        Flag = ItemFlag.Empty;
        if (SourceTemplate.TradeBlock)
            Flag |= ItemFlag.UNTRADEABLE;
        if (SourceTemplate.AccountSharable)
            Flag |= ItemFlag.ACCOUNT_SHARING;

        UniqueId = uniqueId <= 0 ? Yitter.IdGenerator.YitIdHelper.NextId() : uniqueId;
    }

    public virtual Item copy()
    {
        Item ret = new Item(id, position, quantity, UniqueId);
        CopyItemProps(ret);
        return ret;
    }

    protected void CopyItemProps(Item input)
    {
        input.quantity = quantity;
        input.position = position;
        input.id = id;

        input.flag = flag;
        input.owner = owner;
        input.expiration = expiration;
        input.giftFrom = giftFrom;
        input.sn = sn;
        input.itemLog = new(itemLog);
        input.Properties = Properties;
    }

    public void setPosition(short position)
    {
        this.position = position;
    }

    public virtual void setQuantity(short quantity)
    {
        this.quantity = quantity;
    }


    public int getItemId()
    {
        return id;
    }

    public virtual long getCashId()
    {
        // 非Pet、Ring的一些普通现金道具需要cashid？数据库并没有存放，只是临时使用？
        //log.Debug("getCashId, PetId: {PetId}, RingId: {RingId}, {StactTrace}", PetId, (this as Equip)?.getRingId(), new StackTrace());
        return UniqueId;
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
    {
        return 2;
    }

    public string getOwner()
    {
        return owner;
    }

    public void setOwner(string? owner)
    {
        this.owner = owner ?? "";
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

    public override bool Equals(object? obj)
    {
        if (obj is not Item item)
        {
            return false;
        }

        return UniqueId == item.UniqueId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId);
    }

    public override string ToString()
    {
        return "Item: " + id + " quantity: " + quantity;
    }

    public List<string> getItemLog()
    {
        return itemLog.ToList();
    }

    public short getFlag()
    {
        return flag;
    }

    public void setFlag(short b)
    {
        this.flag = b;
    }

    void LockItemInner(long expire)
    {
        Flag |= ItemFlag.LOCK;
        LockExpiration = expire;
    }
    public void LockItem(long expire)
    {
        if (Store is AbstractInventory inv)
        {
            inv.ForceUpdate(this, i => i.LockItemInner(expire));
        }
        else
        {
            LockItemInner(expire);
        }
    }

    void UnlockInner()
    {
        Flag &= ~ItemFlag.LOCK;
        LockExpiration = 0;
    }
    public void Unlock()
    {
        if (Store is AbstractInventory inv)
        {
            inv.ForceUpdate(this, i => i.UnlockInner());
        }
        else
        {
            UnlockInner();
        }
    }

    public long getExpiration()
    {
        return expiration;
    }

    protected virtual void SetExpirationInner(long expire)
    {
        expiration = expire;
    }

    public void setExpiration(long expire)
    {
        if (Store is AbstractInventory inv)
        {
            inv.ForceUpdate(this, i => SetExpirationInner(expire));
        }
        else
        {
            SetExpirationInner(expire);
        }
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

    public void setGiftFrom(string? giftFrom)
    {
        this.giftFrom = giftFrom ?? "";
    }
    public bool isUntradeable()
    {
        return Flag.HasFlag(ItemFlag.UNTRADEABLE)
            || (ItemInformationProvider.getInstance().isDropRestricted(this.getItemId()) && !KarmaManipulator.hasKarmaFlag(this));
    }

    public int GetSortKey()
    {
        if (SourceTemplate is not BulletItemTemplate bulletItemTemplate)
            return id;

        var typeTag = id / 10000;
        return typeTag * 10000 + 1000 - bulletItemTemplate.IncPAD;
    }

    /// <summary>
    /// 可以堆叠
    /// </summary>
    /// <param name="anotherItem"></param>
    /// <param name="chr"></param>
    /// <returns></returns>
    public virtual bool CanStack(Item anotherItem, Player chr)
    {
        return getItemId() == anotherItem.getItemId()
            && !ItemConstants.isRechargeable(getItemId())
            && getQuantity() < ItemInformationProvider.getInstance().getSlotMax(chr.Client, getItemId())
            && getOwner() == anotherItem.getOwner()
            && getExpiration() == anotherItem.getExpiration()
            && Flag == anotherItem.Flag
            && !Flag.HasFlag(ItemFlag.LOCK);
    }
}

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
using Application.Core.Game.Items;
using client.inventory.manipulator;
using Org.BouncyCastle.Ocsp;

namespace client.inventory;

public class Item : IComparable<Item>, IItemProp
{
    protected ILogger log;
    protected int id;
    protected int sn;
    protected short position;
    protected short quantity;
    public long PetId { get; set; } = -1;

    protected string owner = "";
    protected List<string> itemLog;
    protected short flag;
    protected long expiration = -1;
    protected string giftFrom = "";

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

    public virtual Item copy()
    {
        Item ret = new Item(id, position, quantity);
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
    }

    /// <summary>
    /// 创建一个还不存在的道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public static Item CreateVirtualItem(int itemId, short quantity)
    {
        return new Item(itemId, 0, quantity);
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
    long? tempCashId;
    public virtual long getCashId()
    {
        // 非Pet、Ring的一些普通现金道具需要cashid？数据库并没有存放，只是临时使用？
        //log.Debug("getCashId, PetId: {PetId}, RingId: {RingId}, {StactTrace}", PetId, (this as Equip)?.getRingId(), new StackTrace());
        return tempCashId ??= Yitter.IdGenerator.YitIdHelper.NextId();
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

    public void setOwner(string owner)
    {
        this.owner = owner;
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
    public bool isUntradeable()
    {
        return ((this.getFlag() & ItemConstants.UNTRADEABLE) == ItemConstants.UNTRADEABLE)
            || (ItemInformationProvider.getInstance().isDropRestricted(this.getItemId()) && !KarmaManipulator.hasKarmaFlag(this));
    }
}

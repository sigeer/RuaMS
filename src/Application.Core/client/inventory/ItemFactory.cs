/*
 This file is part of the OdinMS  Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License version 3
 as published by the Free Software Foundation. You may not use, modify
 or distribute this program under any other version of the
 GNU Affero General Public License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Microsoft.EntityFrameworkCore;

namespace client.inventory;

/**
 * @author Flav
 */
public class ItemFactory : EnumClass
{
    /// <summary>
    /// 背包（已装备）
    /// </summary>
    public static readonly ItemFactory INVENTORY = new ItemFactory(1, false);
    /// <summary>
    /// 仓库
    /// </summary>
    public static readonly ItemFactory STORAGE = new ItemFactory(2, true);
    /// <summary>
    /// 现金道具仓库？
    /// </summary>
    public static readonly ItemFactory CASH_EXPLORER = new ItemFactory(3, true);
    public static readonly ItemFactory CASH_CYGNUS = new ItemFactory(4, true);
    public static readonly ItemFactory CASH_ARAN = new ItemFactory(5, true);
    /// <summary>
    /// 雇佣商人
    /// </summary>
    public static readonly ItemFactory MERCHANT = new ItemFactory(6, false);
    public static readonly ItemFactory CASH_OVERALL = new ItemFactory(7, true);
    public static readonly ItemFactory MARRIAGE_GIFTS = new ItemFactory(8, false);
    /// <summary>
    /// 快递
    /// </summary>
    public static readonly ItemFactory DUEY = new(9, false);
    /// <summary>
    /// MTS
    /// </summary>
    public static readonly ItemFactory MTS = new(10, false);
    private int value;
    private bool account;

    private static int lockCount = 400;
    private static object[] locks = new object[lockCount];  // thanks Masterrulax for pointing out a bottleneck issue here

    static ItemFactory()
    {
        for (int i = 0; i < lockCount; i++)
        {
            locks[i] = new object();
        }
    }

    ItemFactory(int value, bool account)
    {
        this.value = value;
        this.account = account;
    }

    public static ItemFactory GetItemFactory(int value)
    {
        if (value == 1)
            return INVENTORY;
        if (value == 2)
            return STORAGE;
        if (value == 3)
            return CASH_EXPLORER;
        if (value == 4)
            return CASH_CYGNUS;
        if (value == 5)
            return CASH_ARAN;
        if (value == 6)
            return MERCHANT;
        if (value == 7)
            return CASH_OVERALL;
        if (value == 8)
            return MARRIAGE_GIFTS;
        if (value == 9)
            return DUEY;
        throw new BusinessFatalException($"不存在的道具分类 {value}");
    }

    public int getValue()
    {
        return value;
    }
    public bool IsAccount => account;


    private static Equip loadEquipFromResultSet(EquipItemModelFromDB rs)
    {
        Equip equip = new Equip(rs.Itemid, rs.Position);
        equip.setOwner(rs.Owner);
        equip.setQuantity(rs.Quantity);
        equip.setAcc(rs.Acc);
        equip.setAvoid(rs.Avoid);
        equip.setDex(rs.Dex);
        equip.setHands(rs.Hands);
        equip.setHp(rs.Hp);
        equip.setInt(rs.Int);
        equip.setJump(rs.Jump);
        equip.setVicious(rs.Vicious);
        equip.setFlag(rs.Flag);
        equip.setLuk(rs.Luk);
        equip.setMatk(rs.Matk);
        equip.setMdef(rs.Mdef);
        equip.setMp(rs.Mp);
        equip.setSpeed(rs.Speed);
        equip.setStr(rs.Str);
        equip.setWatk(rs.Watk);
        equip.setWdef(rs.Wdef);
        equip.setUpgradeSlots(rs.Upgradeslots);
        equip.setLevel(rs.Level);
        equip.setItemExp(rs.Itemexp);
        equip.setItemLevel(rs.Itemlevel);
        equip.setExpiration(rs.Expiration);
        equip.setGiftFrom(rs.GiftFrom);

        return equip;
    }

    /// <summary>
    /// 加载已穿戴的装备
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns>Item</returns>
    public static List<Equip> loadEquippedItems(int characterId)
    {
        var equipedType = InventoryType.EQUIPPED.getType();

        using var dbContext = new DBContext();
        var dataList = (from a in dbContext.Inventoryitems
                        join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                        from bs in bss.DefaultIfEmpty()
                        where a.Characterid == characterId
                        where a.Inventorytype == equipedType
                        select new EquipItemModelFromDB(a, bs)).ToList();

        return dataList.Select(x => loadEquipFromResultSet(x)).ToList();
    }
}


public class EquipItemModelFromDB
{
    public int CharacterId { get; set; }
    public int Itemid { get; internal set; }
    public short Position { get; internal set; }
    public string Owner { get; internal set; } = null!;
    public short Quantity { get; internal set; }
    public int Acc { get; internal set; }
    public int Avoid { get; internal set; }
    public int Dex { get; internal set; }
    public int Hands { get; internal set; }
    public int Hp { get; internal set; }
    public int Int { get; internal set; }
    public int Jump { get; internal set; }
    public int Vicious { get; internal set; }
    public short Flag { get; internal set; }
    public int Luk { get; internal set; }
    public int Matk { get; internal set; }
    public int Mdef { get; internal set; }
    public int Mp { get; internal set; }
    public int Speed { get; internal set; }
    public int Str { get; internal set; }
    public int Watk { get; internal set; }
    public int Wdef { get; internal set; }
    public int Upgradeslots { get; internal set; }
    public byte Level { get; internal set; }
    public byte Itemlevel { get; internal set; }
    public string GiftFrom { get; internal set; } = null!;
    public int Itemexp { get; internal set; }
    public long Expiration { get; internal set; }
    public long Ringid { get; internal set; } = -1;
    public sbyte Inventorytype { get; set; }
    public long PetId { get; internal set; }
    public int Inventoryitemid { get; internal set; }

    public EquipItemModelFromDB()
    {

    }
    public EquipItemModelFromDB(Inventoryitem a, Inventoryequipment? b)
    {
        if (b != null)
        {
            Hp = b.Hp;
            Acc = b.Acc;
            Avoid = b.Avoid;
            Dex = b.Dex;
            Hands = b.Hands;
            Int = b.Int;
            Jump = b.Jump;
            Level = b.Level;
            Luk = b.Luk;
            Matk = b.Matk;
            Mdef = b.Mdef;
            Mp = b.Mp;
            Itemlevel = b.Itemlevel;
            Itemexp = b.Itemexp;

            Ringid = b.RingId;
            Speed = b.Speed;
            Str = b.Str;
            Upgradeslots = b.Upgradeslots;
            Vicious = b.Vicious;
            Watk = b.Watk;
            Wdef = b.Wdef;
        }

        Inventorytype = a.Inventorytype;
        CharacterId = a.Characterid ?? 0;
        Expiration = a.Expiration;
        Flag = a.Flag;
        GiftFrom = a.GiftFrom;
        Inventoryitemid = a.Inventoryitemid;
        Itemid = a.Itemid;
        Owner = a.Owner;
        PetId = a.Petid;
        Position = a.Position;
        Quantity = a.Quantity;

    }
}

public record ItemInventoryType(Item Item, InventoryType Type);
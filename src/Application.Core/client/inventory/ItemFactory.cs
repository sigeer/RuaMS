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
}
public record ItemInventoryType(Item Item, InventoryType Type);
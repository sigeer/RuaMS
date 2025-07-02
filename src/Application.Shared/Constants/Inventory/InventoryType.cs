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
namespace Application.Shared.Constants.Inventory;

/**
 * @author Matze
 */
public enum InventoryType
{
    UNDEFINED = 0,
    /// <summary>
    /// 装备
    /// </summary>
    EQUIP = 1,
    /// <summary>
    /// 消耗
    /// </summary>
    USE = 2,
    /// <summary>
    /// 设置（椅子）
    /// </summary>
    SETUP = 3,
    /// <summary>
    /// 其他
    /// </summary>
    ETC = 4,
    /// <summary>
    /// 特殊（现金）
    /// </summary>
    CASH = 5,
    CANHOLD = 6,   //Proof-guard for inserting after removal checks
    /// <summary>
    /// 穿在身上的装备
    /// </summary>
    EQUIPPED = -1 //Seems nexon screwed something when removing an item T_T
}

public class InventoryTypeFactory
{
    public readonly static InventoryType[] All = Enum.GetValues<InventoryType>();
}

public static class InventoryTypeUtils
{
    public static InventoryType getByType(this sbyte id)
    {
        return (InventoryType)id;
    }

    public static InventoryType GetByType(this int id)
    {
        return (InventoryType)id;
    }

    public static int ordinal(this InventoryType val)
    {
        var d = val.getType();
        if (d == -1)
            return 7;
        return d;
    }

    public static sbyte getType(this InventoryType type)
    {
        return (sbyte)type;
    }

    public static short getBitfieldEncoding(this InventoryType type)
    {
        return (short)(2 << (byte)type);
    }


    public static InventoryType getByWZName(string name)
    {
        return name switch
        {
            "Install" => InventoryType.SETUP,
            "Consume" => InventoryType.USE,
            "Etc" => InventoryType.ETC,
            "Cash" => InventoryType.CASH,
            "Pet" => InventoryType.CASH,
            _ => InventoryType.UNDEFINED
        };
    }
}

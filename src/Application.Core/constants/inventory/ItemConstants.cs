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


using client.inventory;
using constants.id;

namespace constants.inventory;

/**
 * @author Jay Estrella
 * @author Ronan
 */
public class ItemConstants
{
    protected static Dictionary<int, InventoryType> inventoryTypeCache = new();

    public const short LOCK = 0x01;
    public const short SPIKES = 0x02;
    public const short KARMA_USE = 0x02;
    public const short COLD = 0x04;
    public const short UNTRADEABLE = 0x08;
    public const short KARMA_EQP = 0x10;
    public const short SANDBOX = 0x40;             // let 0x40 until it's proven something uses this
    public const short PET_COME = 0x80;
    public const short ACCOUNT_SHARING = 0x100;
    public const short MERGE_UNTRADEABLE = 0x200;

    public static bool EXPIRING_ITEMS = true;
    public static HashSet<int> permanentItemids = new();

    static ItemConstants()
    {
        // i ain't going to open one gigantic itemid cache just for 4 perma itemids, no way!
        foreach (int petItemId in ItemId.getPermaPets())
        {
            permanentItemids.Add(petItemId);
        }
    }

    public static int getFlagByInt(int type)
    {
        if (type == 128)
        {
            return PET_COME;
        }
        else if (type == 256)
        {
            return ACCOUNT_SHARING;
        }
        return 0;
    }

    public static bool isThrowingStar(int itemId)
    {
        return itemId / 10000 == 207;
    }

    public static bool isBullet(int itemId)
    {
        return itemId / 10000 == 233;
    }

    public static bool isPotion(int itemId)
    {
        return itemId / 1000 == 2000;
    }

    public static bool isFood(int itemId)
    {
        int useType = itemId / 1000;
        return useType == 2022 || useType == 2010 || useType == 2020;
    }

    public static bool isConsumable(int itemId)
    {
        return isPotion(itemId) || isFood(itemId);
    }

    public static bool isRechargeable(int itemId)
    {
        return isThrowingStar(itemId) || isBullet(itemId);
    }

    public static bool isArrowForCrossBow(int itemId)
    {
        return itemId / 1000 == 2061;
    }

    public static bool isArrowForBow(int itemId)
    {
        return itemId / 1000 == 2060;
    }

    public static bool isArrow(int itemId)
    {
        return isArrowForBow(itemId) || isArrowForCrossBow(itemId);
    }

    public static bool isPet(int itemId)
    {
        return itemId / 1000 == 5000;
    }

    public static bool isExpirablePet(int itemId)
    {
        return YamlConfig.config.server.USE_ERASE_PET_ON_EXPIRATION || itemId == ItemId.PET_SNAIL;
    }

    public static bool isPermanentItem(int itemId)
    {
        return permanentItemids.Contains(itemId);
    }

    public static bool isNewYearCardEtc(int itemId)
    {
        return itemId / 10000 == 430;
    }

    public static bool isNewYearCardUse(int itemId)
    {
        return itemId / 10000 == 216;
    }

    public static bool isAccessory(int itemId)
    {
        return itemId >= 1110000 && itemId < 1140000;
    }

    public static bool isTaming(int itemId)
    {
        int itemType = itemId / 1000;
        return itemType == 1902 || itemType == 1912;
    }

    public static bool isTownScroll(int itemId)
    {
        return itemId >= 2030000;
    }

    public static bool isCleanSlate(int scrollId)
    {
        return scrollId > 2048999 && scrollId < 2049004;
    }

    public static bool isModifierScroll(int scrollId)
    {
        return scrollId == ItemId.SPIKES_SCROLL || scrollId == ItemId.COLD_PROTECTION_SCROLl;
    }

    public static bool isFlagModifier(int scrollId, short flag)
    {
        if (scrollId == ItemId.COLD_PROTECTION_SCROLl && ((flag & ItemConstants.COLD) == ItemConstants.COLD))
        {
            return true;
        }
        return scrollId == ItemId.SPIKES_SCROLL && ((flag & ItemConstants.SPIKES) == ItemConstants.SPIKES);
    }

    public static bool isChaosScroll(int scrollId)
    {
        return scrollId >= 2049100 && scrollId <= 2049103;
    }

    public static bool isRateCoupon(int itemId)
    {
        int itemType = itemId / 1000;
        return itemType == 5211 || itemType == 5360;
    }

    public static bool isExpCoupon(int couponId)
    {
        return couponId / 1000 == 5211;
    }

    public static bool isPartyItem(int itemId)
    {
        return itemId >= 2022430 && itemId <= 2022433 || itemId >= 2022160 && itemId <= 2022163;
    }

    public static bool isHiredMerchant(int itemId)
    {
        return itemId / 10000 == 503;
    }

    public static bool isPlayerShop(int itemId)
    {
        return itemId / 10000 == 514;
    }

    public static InventoryType getInventoryType(int itemId)
    {
        if (inventoryTypeCache.ContainsKey(itemId))
        {
            return inventoryTypeCache.GetValueOrDefault(itemId);
        }

        InventoryType ret = InventoryType.UNDEFINED;

        byte type = (byte)(itemId / 1000000);
        if (type >= 1 && type <= 5)
        {
            ret = InventoryTypeUtils.getByType((sbyte)type);
        }

        inventoryTypeCache.Add(itemId, ret);
        return ret;
    }

    public static bool isMakerReagent(int itemId)
    {
        return itemId / 10000 == 425;
    }

    public static bool isOverall(int itemId)
    {
        return itemId / 10000 == 105;
    }

    public static bool isCashStore(int itemId)
    {
        int itemType = itemId / 10000;
        return itemType == 503 || itemType == 514;
    }

    public static bool isMapleLife(int itemId)
    {
        int itemType = itemId / 10000;
        return itemType == 543 && itemId != 5430000;
    }

    public static bool isWeapon(int itemId)
    {
        return itemId >= 1302000 && itemId < 1493000;
    }

    public static bool isEquipment(int itemId)
    {
        return itemId < 2000000 && itemId != 0;
    }

    public static bool isFishingChair(int itemId)
    {
        return itemId == ItemId.FISHING_CHAIR;
    }

    public static bool isMedal(int itemId)
    {
        return itemId >= 1140000 && itemId < 1143000;
    }

    public static bool isFace(int itemId)
    {
        return itemId >= 20000 && itemId < 22000;
    }

    public static bool isHair(int itemId)
    {
        return itemId >= 30000 && itemId < 35000;
    }
}

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


using Application.Core.Channel.Infrastructures;
using Application.Core.Game.Skills;
using Application.Core.model;
using Application.Core.ServerTransports;
using client.autoban;
using client.inventory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Channel.DataProviders;


/**
 * @author Matze
 */
public class ItemInformationProvider : WZDataBootstrap, IStaticService
{
    private static ItemInformationProvider? _instance;

    public static ItemInformationProvider getInstance()
    {
        return _instance ?? throw new BusinessFatalException("ItemInformationProvider 未注册");
    }

    readonly IChannelServerTransport _transport;
    readonly IMemoryCache _cache;
    public ItemInformationProvider(IChannelServerTransport transport, IMemoryCache cache, ILogger<WZDataBootstrap> logger) : base(logger)
    {
        Name = "物品数据";

        _transport = transport;
        _cache = cache;

        itemData = DataProviderFactory.getDataProvider(WZFiles.ITEM);
        equipData = DataProviderFactory.getDataProvider(WZFiles.CHARACTER);
        stringData = DataProviderFactory.getDataProvider(WZFiles.STRING);
        etcData = DataProviderFactory.getDataProvider(WZFiles.ETC);
        cashStringData = stringData.getData("Cash.img");
        consumeStringData = stringData.getData("Consume.img");
        eqpStringData = stringData.getData("Eqp.img");
        etcStringData = stringData.getData("Etc.img");
        insStringData = stringData.getData("Ins.img");
        petStringData = stringData.getData("Pet.img");

        isQuestItemCache.Add(0, false);
        isPartyQuestItemCache.Add(0, false);
    }

    public void Register(IServiceProvider sp)
    {
        _instance = sp.GetService<ItemInformationProvider>() ?? throw new BusinessFatalException("ItemInformationProvider 未注册");
    }

    protected override void LoadDataInternal()
    {
        loadCardIdData();
    }

    protected DataProvider itemData;
    protected DataProvider equipData;
    protected DataProvider stringData;
    protected DataProvider etcData;
    protected Data cashStringData;
    protected Data consumeStringData;
    protected Data eqpStringData;
    protected Data etcStringData;
    protected Data insStringData;
    protected Data petStringData;
    protected Dictionary<int, short> slotMaxCache = new();
    protected Dictionary<int, StatEffect> itemEffects = new();
    protected Dictionary<int, Dictionary<string, int>> equipStatsCache = new();
    protected Dictionary<int, Equip> equipCache = new();
    protected Dictionary<int, Data?> equipLevelInfoCache = new();
    protected Dictionary<int, int> equipLevelReqCache = new();
    protected Dictionary<int, int> equipMaxLevelCache = new();
    protected Dictionary<int, List<int>> scrollReqsCache = new();
    protected Dictionary<int, int> wholePriceCache = new();
    protected Dictionary<int, double> unitPriceCache = new();
    protected Dictionary<int, int> projectileWatkCache = new();
    protected Dictionary<int, string?> nameCache = new();
    protected Dictionary<int, string> descCache = new();
    protected Dictionary<int, string?> msgCache = new();
    protected Dictionary<int, bool> accountItemRestrictionCache = new();
    protected Dictionary<int, bool> dropRestrictionCache = new();
    protected Dictionary<int, bool> pickupRestrictionCache = new();
    protected Dictionary<int, int> getMesoCache = new();
    protected Dictionary<int, int> monsterBookID = new();
    protected Dictionary<int, bool> untradeableCache = new();
    protected Dictionary<int, bool> onEquipUntradeableCache = new();
    protected Dictionary<int, ScriptedItem> scriptedItemCache = new();
    protected Dictionary<int, bool> karmaCache = new();
    protected Dictionary<int, int> triggerItemCache = new();
    protected Dictionary<int, int> expCache = new();
    protected Dictionary<int, int> createItemCache = new();
    protected Dictionary<int, int> mobItemCache = new();
    protected Dictionary<int, int> useDelayCache = new();
    protected Dictionary<int, int> mobHPCache = new();
    protected Dictionary<int, int> levelCache = new();
    protected Dictionary<int, KeyValuePair<int, List<RewardItem>>> rewardCache = new();
    protected List<ItemInfoBase> itemNameCache = new();
    protected List<ItemInfoBase> etcItemCache = new List<ItemInfoBase>();
    protected Dictionary<int, bool> consumeOnPickupCache = new();
    protected Dictionary<int, bool> isQuestItemCache = new();
    protected Dictionary<int, bool> isPartyQuestItemCache = new();
    protected Dictionary<int, ItemMessage> replaceOnExpireCache = new();
    protected Dictionary<int, string> equipmentSlotCache = new();
    protected Dictionary<int, bool> noCancelMouseCache = new();


    protected Dictionary<int, ItemSkillDataPair> SkillUpgreadCache = [];
    protected Dictionary<int, KeyValuePair<int, HashSet<int>>?> cashPetFoodCache = new();
    protected Dictionary<int, QuestConsItem?> questItemConsCache = new();


    public List<ItemInfoBase> getAllItems()
    {
        if (itemNameCache.Count > 0)
        {
            return itemNameCache;
        }
        List<ItemInfoBase> itemPairs = new();
        var itemsData = stringData.getData("Cash.img");
        foreach (Data itemFolder in itemsData.getChildren())
        {
            //DataTool.getString("name", xxx) : 子标签中的<string name="name">的value值

            itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
        }
        itemsData = stringData.getData("Consume.img");
        foreach (Data itemFolder in itemsData.getChildren())
        {
            itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
        }
        itemsData = stringData.getData("Eqp.img").getChildByPath("Eqp") ?? throw new BusinessResException("Eqp.img/Epq");
        foreach (Data eqpType in itemsData.getChildren())
        {
            foreach (Data itemFolder in eqpType.getChildren())
            {
                itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
            }
        }
        itemsData = stringData.getData("Etc.img").getChildByPath("Etc") ?? throw new BusinessResException("Etc.img/Etc");
        foreach (Data itemFolder in itemsData.getChildren())
        {
            itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
        }
        itemsData = stringData.getData("Ins.img");
        foreach (Data itemFolder in itemsData.getChildren())
        {
            itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
        }
        itemsData = stringData.getData("Pet.img");
        foreach (Data itemFolder in itemsData.getChildren())
        {
            itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
        }

        itemNameCache = itemPairs;
        return itemPairs;
    }

    //public List<ItemInfoBase> getAllEtcItems()
    //{
    //    if (etcItemCache.Count > 0)
    //    {
    //        return etcItemCache;
    //    }

    //    List<ItemInfoBase> itemPairs = new();
    //    Data? itemsData;

    //    itemsData = stringData.getData("Etc.img").getChildByPath("Etc") ?? throw new BusinessResException("Etc.img/Etc");
    //    foreach (Data itemFolder in itemsData.getChildren())
    //    {
    //        itemPairs.Add(new(int.Parse(itemFolder.getName()!), DataTool.getString("name", itemFolder) ?? "NO-NAME"));
    //    }
    //    etcItemCache = itemPairs;
    //    return itemPairs;
    //}

    private Data? getStringData(int itemId)
    {
        string cat = "null";
        Data theData;
        if (itemId >= 5010000)
        {
            theData = cashStringData;
        }
        else if (itemId >= 2000000 && itemId < 3000000)
        {
            theData = consumeStringData;
        }
        else if (itemId >= 1010000 && itemId < 1040000 || itemId >= 1122000 && itemId < 1123000 || itemId >= 1132000 && itemId < 1133000 || itemId >= 1142000 && itemId < 1143000)
        {
            theData = eqpStringData;
            cat = "Eqp/Accessory";
        }
        else if (itemId >= 1000000 && itemId < 1010000)
        {
            theData = eqpStringData;
            cat = "Eqp/Cap";
        }
        else if (itemId >= 1102000 && itemId < 1103000)
        {
            theData = eqpStringData;
            cat = "Eqp/Cape";
        }
        else if (itemId >= 1040000 && itemId < 1050000)
        {
            theData = eqpStringData;
            cat = "Eqp/Coat";
        }
        else if (ItemConstants.isFace(itemId))
        {
            theData = eqpStringData;
            cat = "Eqp/Face";
        }
        else if (itemId >= 1080000 && itemId < 1090000)
        {
            theData = eqpStringData;
            cat = "Eqp/Glove";
        }
        else if (ItemConstants.isHair(itemId))
        {
            theData = eqpStringData;
            cat = "Eqp/Hair";
        }
        else if (itemId >= 1050000 && itemId < 1060000)
        {
            theData = eqpStringData;
            cat = "Eqp/Longcoat";
        }
        else if (itemId >= 1060000 && itemId < 1070000)
        {
            theData = eqpStringData;
            cat = "Eqp/Pants";
        }
        else if (itemId >= 1802000 && itemId < 1842000)
        {
            theData = eqpStringData;
            cat = "Eqp/PetEquip";
        }
        else if (itemId >= 1112000 && itemId < 1120000)
        {
            theData = eqpStringData;
            cat = "Eqp/Ring";
        }
        else if (itemId >= 1092000 && itemId < 1100000)
        {
            theData = eqpStringData;
            cat = "Eqp/Shield";
        }
        else if (itemId >= 1070000 && itemId < 1080000)
        {
            theData = eqpStringData;
            cat = "Eqp/Shoes";
        }
        else if (itemId >= 1900000 && itemId < 2000000)
        {
            theData = eqpStringData;
            cat = "Eqp/Taming";
        }
        else if (itemId >= 1300000 && itemId < 1800000)
        {
            theData = eqpStringData;
            cat = "Eqp/Weapon";
        }
        else if (itemId >= 4000000 && itemId < 5000000)
        {
            theData = etcStringData;
            cat = "Etc";
        }
        else if (itemId >= 3000000 && itemId < 4000000)
        {
            theData = insStringData;
        }
        else if (ItemConstants.isPet(itemId))
        {
            theData = petStringData;
        }
        else
        {
            return null;
        }
        if (cat.Equals("null", StringComparison.OrdinalIgnoreCase))
            return theData?.getChildByPath(itemId.ToString());
        else
        {
            return theData.getChildByPath(cat + "/" + itemId);
        }
    }

    public bool noCancelMouse(int itemId)
    {
        if (noCancelMouseCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        var item = getItemData(itemId);
        if (item == null)
        {
            noCancelMouseCache.Add(itemId, false);
            return false;
        }

        bool blockMouse = DataTool.getIntConvert("info/noCancelMouse", item, 0) == 1;
        noCancelMouseCache.Add(itemId, blockMouse);
        return blockMouse;
    }

    Dictionary<int, Data?> itemCache = [];

    private Data? getItemData(int itemId)
    {
        if (itemCache.TryGetValue(itemId, out var value))
            return value;

        Data? ret = null;
        string idStr = "0" + itemId;
        DataDirectoryEntry root = itemData.getRoot();
        foreach (DataDirectoryEntry topDir in root.getSubdirectories())
        {
            foreach (DataFileEntry iFile in topDir.getFiles())
            {
                if (iFile.getName() == idStr.Substring(0, 4) + ".img")
                {
                    ret = itemData.getData(topDir.getName() + "/" + iFile.getName());
                    if (ret == null)
                    {
                        itemCache[itemId] = null;
                        return null;
                    }
                    ret = ret.getChildByPath(idStr);
                    itemCache[itemId] = ret;
                    return ret;
                }
                else if (iFile.getName() == idStr.Substring(1) + ".img")
                {
                    ret = itemData.getData(topDir.getName() + "/" + iFile.getName());
                    itemCache[itemId] = ret;
                    return ret;
                }
            }
        }
        root = equipData.getRoot();
        foreach (DataDirectoryEntry topDir in root.getSubdirectories())
        {
            foreach (DataFileEntry iFile in topDir.getFiles())
            {
                if (iFile.getName() == idStr + ".img")
                {
                    ret = equipData.getData(topDir.getName() + "/" + iFile.getName());
                    itemCache[itemId] = ret;
                    return ret;
                }
            }
        }
        return ret;
    }

    private static int getExtraSlotMaxFromPlayer(IChannelClient c, int itemId)
    {
        int ret = 0;

        // thanks GMChuck for detecting player sensitive data being cached into getSlotMax
        if (ItemConstants.isThrowingStar(itemId))
        {
            if (c.OnlinedCharacter.getJob().isA(Job.NIGHTWALKER1))
            {
                ret += c.OnlinedCharacter.getSkillLevel(SkillFactory.GetSkillTrust(NightWalker.CLAW_MASTERY)) * 10;
            }
            else
            {
                ret += c.OnlinedCharacter.getSkillLevel(SkillFactory.GetSkillTrust(Assassin.CLAW_MASTERY)) * 10;
            }
        }
        else if (ItemConstants.isBullet(itemId))
        {
            ret += c.OnlinedCharacter.getSkillLevel(SkillFactory.GetSkillTrust(Gunslinger.GUN_MASTERY)) * 10;
        }

        return ret;
    }

    public short getSlotMax(IChannelClient c, int itemId)
    {
        if (slotMaxCache.TryGetValue(itemId, out var slotMax))
            return (short)(slotMax + getExtraSlotMaxFromPlayer(c, itemId));

        short ret = 0;
        var item = getItemData(itemId);
        if (item != null)
        {
            var smEntry = item.getChildByPath("info/slotMax");
            if (smEntry == null)
            {
                if (ItemConstants.getInventoryType(itemId).getType() == InventoryType.EQUIP.getType())
                {
                    ret = 1;
                }
                else
                {
                    ret = 100;
                }
            }
            else
            {
                ret = (short)DataTool.getInt(smEntry);
            }
        }

        slotMaxCache.Add(itemId, ret);
        return (short)(ret + getExtraSlotMaxFromPlayer(c, itemId));
    }

    public int getMeso(int itemId)
    {
        if (getMesoCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        var item = getItemData(itemId);
        if (item == null)
        {
            return -1;
        }

        var pData = item.getChildByPath("info/meso");
        if (pData == null)
        {
            return -1;
        }
        var pEntry = DataTool.getInt(pData);
        getMesoCache.Add(itemId, pEntry);
        return pEntry;
    }

    private static double getRoundedUnitPrice(double unitPrice, int max)
    {
        double intPart = Math.Floor(unitPrice);
        double fractPart = unitPrice - intPart;
        if (fractPart == 0.0)
        {
            return intPart;
        }

        double fractMask = 0.0;
        double lastFract, curFract = 1.0;
        int i = 1;

        do
        {
            lastFract = curFract;
            curFract /= 2;

            if (fractPart == curFract)
            {
                break;
            }
            else if (fractPart > curFract)
            {
                fractMask += curFract;
                fractPart -= curFract;
            }

            i++;
        } while (i <= max);

        if (i > max)
        {
            lastFract = curFract;
            curFract = 0.0;
        }

        if (Math.Abs(fractPart - curFract) < Math.Abs(fractPart - lastFract))
        {
            return intPart + fractMask + curFract;
        }
        else
        {
            return intPart + fractMask + lastFract;
        }
    }

    private KeyValuePair<int, double> getItemPriceData(int itemId)
    {
        var item = getItemData(itemId);
        if (item == null)
        {
            wholePriceCache.AddOrUpdate(itemId, -1);
            unitPriceCache.AddOrUpdate(itemId, 0.0);
            return new(-1, 0.0);
        }

        int pEntry = -1;
        var pData = item.getChildByPath("info/price");
        if (pData != null)
        {
            pEntry = DataTool.getInt(pData);
        }

        double fEntry = 0.0f;
        pData = item.getChildByPath("info/unitPrice");
        if (pData != null)
        {
            try
            {
                fEntry = getRoundedUnitPrice(DataTool.getDouble(pData), 5);
            }
            catch (Exception)
            {
                fEntry = DataTool.getInt(pData);
            }
        }

        wholePriceCache.AddOrUpdate(itemId, pEntry);
        unitPriceCache.AddOrUpdate(itemId, fEntry);
        return new(pEntry, fEntry);
    }

    public int getWholePrice(int itemId)
    {
        if (wholePriceCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        return getItemPriceData(itemId).Key;
    }

    public double getUnitPrice(int itemId)
    {
        if (unitPriceCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        return getItemPriceData(itemId).Value;
    }

    public int getPrice(int itemId, int quantity)
    {
        int retPrice = getWholePrice(itemId);
        if (retPrice == -1)
        {
            return -1;
        }

        if (!ItemConstants.isRechargeable(itemId))
        {
            retPrice *= quantity;
        }
        else
        {
            retPrice += (int)Math.Ceiling(quantity * getUnitPrice(itemId));
        }

        return retPrice;
    }

    public ItemMessage getReplaceOnExpire(int itemId)
    {
        // thanks to GabrielSin
        if (replaceOnExpireCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        var data = getItemData(itemId);
        int itemReplacement = DataTool.getInt("info/replace/itemid", data, 0);
        string msg = DataTool.getString("info/replace/msg", data) ?? "";

        var ret = new ItemMessage(itemReplacement, msg);
        replaceOnExpireCache.Add(itemId, ret);

        return ret;
    }

    protected string? getEquipmentSlot(int itemId)
    {
        if (equipmentSlotCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        var item = getItemData(itemId);
        if (item == null)
        {
            return null;
        }

        var info = item.getChildByPath("info");

        if (info == null)
        {
            return null;
        }

        var ret = DataTool.getString("islot", info) ?? "";

        equipmentSlotCache.Add(itemId, ret);

        return ret;
    }

    public Dictionary<string, int>? getEquipStats(int itemId)
    {
        if (equipStatsCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        Dictionary<string, int> ret = new();
        var item = getItemData(itemId);
        if (item == null)
        {
            return null;
        }
        var info = item.getChildByPath("info");
        if (info == null)
        {
            return null;
        }
        foreach (Data data in info.getChildren())
        {
            if (data.getName()?.StartsWith("inc") ?? false)
                ret.AddOrUpdate(data.getName()!.Substring(3), DataTool.getIntConvert(data));
        }
        /*else if (data.getName().startsWith("req"))
         ret.Add(data.getName(), DataTool.getInt(data.getName(), info, 0));*/

        ret.AddOrUpdate("reqJob", DataTool.getInt("reqJob", info, 0));
        ret.AddOrUpdate("reqLevel", DataTool.getInt("reqLevel", info, 0));
        ret.AddOrUpdate("reqDEX", DataTool.getInt("reqDEX", info, 0));
        ret.AddOrUpdate("reqSTR", DataTool.getInt("reqSTR", info, 0));
        ret.AddOrUpdate("reqINT", DataTool.getInt("reqINT", info, 0));
        ret.AddOrUpdate("reqLUK", DataTool.getInt("reqLUK", info, 0));
        ret.AddOrUpdate("reqPOP", DataTool.getInt("reqPOP", info, 0));
        ret.AddOrUpdate("cash", DataTool.getInt("cash", info, 0));
        ret.AddOrUpdate("tuc", DataTool.getInt("tuc", info, 0));
        ret.AddOrUpdate("cursed", DataTool.getInt("cursed", info, 0));
        ret.AddOrUpdate("success", DataTool.getInt("success", info, 0));
        ret.AddOrUpdate("fs", DataTool.getInt("fs", info, 0));
        equipStatsCache.Add(itemId, ret);
        return ret;
    }

    public int getEquipLevelReq(int itemId)
    {
        if (equipLevelReqCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        int ret = 0;
        var item = getItemData(itemId);
        if (item != null)
        {
            var info = item.getChildByPath("info");
            if (info != null)
            {
                ret = DataTool.getInt("reqLevel", info, 0);
            }
        }

        equipLevelReqCache.Add(itemId, ret);
        return ret;
    }

    public List<int> getScrollReqs(int itemId)
    {
        if (scrollReqsCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        List<int> ret = new();
        var data = getItemData(itemId);
        data = data?.getChildByPath("req");
        if (data != null)
        {
            foreach (Data req in data.getChildren())
            {
                ret.Add(DataTool.getInt(req));
            }
        }

        scrollReqsCache.Add(itemId, ret);
        return ret;
    }

    public WeaponType getWeaponType(int itemId)
    {
        int cat = itemId / 10000 % 100;
        WeaponType[] type = {
            WeaponType.SWORD1H,
            WeaponType.GENERAL1H_SWING,
            WeaponType.GENERAL1H_SWING,
            WeaponType.DAGGER_OTHER,
            WeaponType.NOT_A_WEAPON,
            WeaponType.NOT_A_WEAPON,
            WeaponType.NOT_A_WEAPON,
            WeaponType.WAND,
            WeaponType.STAFF,
            WeaponType.NOT_A_WEAPON,
            WeaponType.SWORD2H,
            WeaponType.GENERAL2H_SWING,
            WeaponType.GENERAL2H_SWING,
            WeaponType.SPEAR_STAB,
            WeaponType.POLE_ARM_SWING,
            WeaponType.BOW,
            WeaponType.CROSSBOW,
            WeaponType.CLAW,
            WeaponType.KNUCKLE,
            WeaponType.GUN };
        if (cat < 30 || cat > 49)
        {
            return WeaponType.NOT_A_WEAPON;
        }
        return type[cat - 30];
    }

    private static double testYourLuck(double prop, int dices)
    {   // revamped testYourLuck author: David A.
        return Math.Pow(1.0 - prop, dices);
    }

    public static bool rollSuccessChance(double propPercent)
    {
        return Randomizer.nextDouble() >= testYourLuck(propPercent / 100.0, YamlConfig.config.server.SCROLL_CHANCE_ROLLS);
    }

    private static short getMaximumShortMaxIfOverflow(int value1, int value2)
    {
        return (short)Math.Min(NumericConfig.MaxStat, Math.Max(value1, value2));
    }

    private static short getShortMaxIfOverflow(int value)
    {
        return (short)Math.Min(NumericConfig.MaxStat, value);
    }

    private static short chscrollRandomizedStat(int range)
    {
        return (short)Randomizer.rand(-range, range);
    }

    private static void UpdateEquipStat(Func<int> getStat, Action<int> setStat, int range)
    {
        var oldValue = getStat();
        if (oldValue > 0)
        {
            int newValue = oldValue + chscrollRandomizedStat(range);
            int baseValue = YamlConfig.config.server.USE_ENHANCED_CHSCROLL ? oldValue : 0;
            setStat(getMaximumShortMaxIfOverflow(baseValue, newValue));
        }
    }


    public void scrollOptionEquipWithChaos(Equip nEquip, int range, bool option)
    {
        // option: watk, matk, wdef, mdef, spd, jump, hp, mp
        //   stat: dex, luk, str, int, avoid, acc

        if (!option)
        {
            UpdateEquipStat(nEquip.getStr, nEquip.setStr, range);
            UpdateEquipStat(nEquip.getDex, nEquip.setDex, range);
            UpdateEquipStat(nEquip.getInt, nEquip.setInt, range);
            UpdateEquipStat(nEquip.getLuk, nEquip.setLuk, range);
            UpdateEquipStat(nEquip.getAcc, nEquip.setAcc, range);
            UpdateEquipStat(nEquip.getAvoid, nEquip.setAvoid, range);
        }
        else
        {
            UpdateEquipStat(nEquip.getWatk, nEquip.setWatk, range);
            UpdateEquipStat(nEquip.getWdef, nEquip.setWdef, range);
            UpdateEquipStat(nEquip.getMatk, nEquip.setMatk, range);
            UpdateEquipStat(nEquip.getMdef, nEquip.setMdef, range);
            UpdateEquipStat(nEquip.getSpeed, nEquip.setSpeed, range);
            UpdateEquipStat(nEquip.getJump, nEquip.setJump, range);
            UpdateEquipStat(nEquip.getHp, nEquip.setHp, range);
            UpdateEquipStat(nEquip.getMp, nEquip.setMp, range);
        }
    }

    private void scrollEquipWithChaos(Equip nEquip, int range)
    {
        if (YamlConfig.config.server.CHSCROLL_STAT_RATE > 0)
        {
            int temp;
            int curStr, curDex, curInt, curLuk, curWatk, curWdef, curMatk, curMdef, curAcc, curAvoid, curSpeed, curJump, curHp, curMp;

            if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
            {
                curStr = nEquip.getStr();
                curDex = nEquip.getDex();
                curInt = nEquip.getInt();
                curLuk = nEquip.getLuk();
                curWatk = nEquip.getWatk();
                curWdef = nEquip.getWdef();
                curMatk = nEquip.getMatk();
                curMdef = nEquip.getMdef();
                curAcc = nEquip.getAcc();
                curAvoid = nEquip.getAvoid();
                curSpeed = nEquip.getSpeed();
                curJump = nEquip.getJump();
                curHp = nEquip.getHp();
                curMp = nEquip.getMp();
            }
            else
            {
                curStr = short.MinValue;
                curDex = short.MinValue;
                curInt = short.MinValue;
                curLuk = short.MinValue;
                curWatk = short.MinValue;
                curWdef = short.MinValue;
                curMatk = short.MinValue;
                curMdef = short.MinValue;
                curAcc = short.MinValue;
                curAvoid = short.MinValue;
                curSpeed = short.MinValue;
                curJump = short.MinValue;
                curHp = short.MinValue;
                curMp = short.MinValue;
            }

            for (int i = 0; i < YamlConfig.config.server.CHSCROLL_STAT_RATE; i++)
            {
                if (nEquip.getStr() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curStr + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getStr() + chscrollRandomizedStat(range);
                    }

                    curStr = getMaximumShortMaxIfOverflow(temp, curStr);
                }

                if (nEquip.getDex() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curDex + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getDex() + chscrollRandomizedStat(range);
                    }

                    curDex = getMaximumShortMaxIfOverflow(temp, curDex);
                }

                if (nEquip.getInt() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curInt + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getInt() + chscrollRandomizedStat(range);
                    }

                    curInt = getMaximumShortMaxIfOverflow(temp, curInt);
                }

                if (nEquip.getLuk() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curLuk + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getLuk() + chscrollRandomizedStat(range);
                    }

                    curLuk = getMaximumShortMaxIfOverflow(temp, curLuk);
                }

                if (nEquip.getWatk() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curWatk + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getWatk() + chscrollRandomizedStat(range);
                    }

                    curWatk = getMaximumShortMaxIfOverflow(temp, curWatk);
                }

                if (nEquip.getWdef() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curWdef + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getWdef() + chscrollRandomizedStat(range);
                    }

                    curWdef = getMaximumShortMaxIfOverflow(temp, curWdef);
                }

                if (nEquip.getMatk() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curMatk + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getMatk() + chscrollRandomizedStat(range);
                    }

                    curMatk = getMaximumShortMaxIfOverflow(temp, curMatk);
                }

                if (nEquip.getMdef() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curMdef + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getMdef() + chscrollRandomizedStat(range);
                    }

                    curMdef = getMaximumShortMaxIfOverflow(temp, curMdef);
                }

                if (nEquip.getAcc() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curAcc + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getAcc() + chscrollRandomizedStat(range);
                    }

                    curAcc = getMaximumShortMaxIfOverflow(temp, curAcc);
                }

                if (nEquip.getAvoid() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curAvoid + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getAvoid() + chscrollRandomizedStat(range);
                    }

                    curAvoid = getMaximumShortMaxIfOverflow(temp, curAvoid);
                }

                if (nEquip.getSpeed() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curSpeed + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getSpeed() + chscrollRandomizedStat(range);
                    }

                    curSpeed = getMaximumShortMaxIfOverflow(temp, curSpeed);
                }

                if (nEquip.getJump() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curJump + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getJump() + chscrollRandomizedStat(range);
                    }

                    curJump = getMaximumShortMaxIfOverflow(temp, curJump);
                }

                if (nEquip.getHp() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curHp + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getHp() + chscrollRandomizedStat(range);
                    }

                    curHp = getMaximumShortMaxIfOverflow(temp, curHp);
                }

                if (nEquip.getMp() > 0)
                {
                    if (YamlConfig.config.server.USE_ENHANCED_CHSCROLL)
                    {
                        temp = curMp + chscrollRandomizedStat(range);
                    }
                    else
                    {
                        temp = nEquip.getMp() + chscrollRandomizedStat(range);
                    }

                    curMp = getMaximumShortMaxIfOverflow(temp, curMp);
                }
            }

            nEquip.setStr(Math.Max(0, curStr));
            nEquip.setDex(Math.Max(0, curDex));
            nEquip.setInt(Math.Max(0, curInt));
            nEquip.setLuk(Math.Max(0, curLuk));
            nEquip.setWatk(Math.Max(0, curWatk));
            nEquip.setWdef(Math.Max(0, curWdef));
            nEquip.setMatk(Math.Max(0, curMatk));
            nEquip.setMdef(Math.Max(0, curMdef));
            nEquip.setAcc(Math.Max(0, curAcc));
            nEquip.setAvoid(Math.Max(0, curAvoid));
            nEquip.setSpeed(Math.Max(0, curSpeed));
            nEquip.setJump(Math.Max(0, curJump));
            nEquip.setHp(Math.Max(0, curHp));
            nEquip.setMp(Math.Max(0, curMp));
        }
        else
        {
            scrollOptionEquipWithChaos(nEquip, range, false);
            scrollOptionEquipWithChaos(nEquip, range, true);
        }
    }

    /*
        Issue with clean slate found thanks to Masterrulax
        Vicious added in the clean slate check thanks to Crypter (CrypterDEV)
    */
    public bool canUseCleanSlate(Equip equip)
    {
        var eqStats = getEquipStats(equip.getItemId());
        if (eqStats == null || eqStats.GetValueOrDefault("tuc") == 0)
        {
            return false;
        }
        int totalUpgradeCount = eqStats.GetValueOrDefault("tuc");
        int freeUpgradeCount = equip.getUpgradeSlots();
        int viciousCount = equip.getVicious();
        int appliedScrollCount = equip.getLevel();
        return freeUpgradeCount + appliedScrollCount < totalUpgradeCount + viciousCount;
    }

    /// <summary>
    /// 上卷轴
    /// </summary>
    /// <param name="equip"></param>
    /// <param name="scrollId"></param>
    /// <param name="usingWhiteScroll"></param>
    /// <param name="vegaItemId"></param>
    /// <param name="isGM"></param>
    /// <returns></returns>
    public Item? scrollEquipWithId(Item equip, int scrollId, bool usingWhiteScroll, int vegaItemId, bool isGM)
    {
        bool assertGM = isGM && YamlConfig.config.server.USE_PERFECT_GM_SCROLL;

        if (equip is Equip nEquip)
        {
            var stats = getEquipStats(scrollId);

            if (nEquip.getUpgradeSlots() > 0 || ItemConstants.isCleanSlate(scrollId) || assertGM)
            {
                double prop = stats?.GetValueOrDefault("success") ?? throw new BusinessResException($"卷轴没有成功率数据，SrollId = {scrollId}");

                switch (vegaItemId)
                {
                    case ItemId.VEGAS_SPELL_10:
                        if (prop == 10.0f)
                        {
                            prop = 30.0f;
                        }
                        break;
                    case ItemId.VEGAS_SPELL_60:
                        if (prop == 60.0f)
                        {
                            prop = 90.0f;
                        }
                        break;
                    case ItemId.CHAOS_SCROll_60:
                        prop = 100.0f;
                        break;
                }

                if (assertGM || rollSuccessChance(prop))
                {
                    short flag = nEquip.getFlag();
                    switch (scrollId)
                    {
                        case ItemId.SPIKES_SCROLL:
                            flag |= ItemConstants.SPIKES;
                            nEquip.setFlag((byte)flag);
                            break;
                        case ItemId.COLD_PROTECTION_SCROLl:
                            flag |= ItemConstants.COLD;
                            nEquip.setFlag((byte)flag);
                            break;
                        case ItemId.CLEAN_SLATE_1:
                        case ItemId.CLEAN_SLATE_3:
                        case ItemId.CLEAN_SLATE_5:
                        case ItemId.CLEAN_SLATE_20:
                            if (canUseCleanSlate(nEquip))
                            {
                                nEquip.setUpgradeSlots((byte)(nEquip.getUpgradeSlots() + 1));
                            }
                            break;
                        case ItemId.CHAOS_SCROll_60:
                        case ItemId.LIAR_TREE_SAP:
                        case ItemId.MAPLE_SYRUP:
                            scrollEquipWithChaos(nEquip, YamlConfig.config.server.CHSCROLL_STAT_RANGE);
                            break;

                        default:
                            improveEquipStats(nEquip, stats);
                            break;
                    }
                    if (!ItemConstants.isCleanSlate(scrollId))
                    {
                        if (!assertGM && !ItemConstants.isModifierScroll(scrollId))
                        {   // issue with modifier scrolls taking slots found thanks to Masterrulax, justin, BakaKnyx
                            nEquip.setUpgradeSlots((byte)(nEquip.getUpgradeSlots() - 1));
                        }
                        nEquip.setLevel((byte)(nEquip.getLevel() + 1));
                    }
                }
                else
                {
                    if (!YamlConfig.config.server.USE_PERFECT_SCROLLING && !usingWhiteScroll && !ItemConstants.isCleanSlate(scrollId) && !assertGM && !ItemConstants.isModifierScroll(scrollId))
                    {
                        nEquip.setUpgradeSlots((byte)(nEquip.getUpgradeSlots() - 1));
                    }
                    if (Randomizer.nextInt(100) < stats.GetValueOrDefault("cursed"))
                    {
                        return null;
                    }
                }
            }
        }
        return equip;
    }

    public static void improveEquipStats(Equip nEquip, Dictionary<string, int> stats)
    {
        foreach (var stat in stats)
        {
            switch (stat.Key)
            {
                case "STR":
                    nEquip.setStr(getShortMaxIfOverflow(nEquip.getStr() + stat.Value));
                    break;
                case "DEX":
                    nEquip.setDex(getShortMaxIfOverflow(nEquip.getDex() + stat.Value));
                    break;
                case "INT":
                    nEquip.setInt(getShortMaxIfOverflow(nEquip.getInt() + stat.Value));
                    break;
                case "LUK":
                    nEquip.setLuk(getShortMaxIfOverflow(nEquip.getLuk() + stat.Value));
                    break;
                case "PAD":
                    nEquip.setWatk(getShortMaxIfOverflow(nEquip.getWatk() + stat.Value));
                    break;
                case "PDD":
                    nEquip.setWdef(getShortMaxIfOverflow(nEquip.getWdef() + stat.Value));
                    break;
                case "MAD":
                    nEquip.setMatk(getShortMaxIfOverflow(nEquip.getMatk() + stat.Value));
                    break;
                case "MDD":
                    nEquip.setMdef(getShortMaxIfOverflow(nEquip.getMdef() + stat.Value));
                    break;
                case "ACC":
                    nEquip.setAcc(getShortMaxIfOverflow(nEquip.getAcc() + stat.Value));
                    break;
                case "EVA":
                    nEquip.setAvoid(getShortMaxIfOverflow(nEquip.getAvoid() + stat.Value));
                    break;
                case "Speed":
                    nEquip.setSpeed(getShortMaxIfOverflow(nEquip.getSpeed() + stat.Value));
                    break;
                case "Jump":
                    nEquip.setJump(getShortMaxIfOverflow(nEquip.getJump() + stat.Value));
                    break;
                case "MHP":
                    nEquip.setHp(getShortMaxIfOverflow(nEquip.getHp() + stat.Value));
                    break;
                case "MMP":
                    nEquip.setMp(getShortMaxIfOverflow(nEquip.getMp() + stat.Value));
                    break;
                case "afterImage":
                    break;
            }
        }
    }

    public Item getEquipById(int equipId)
    {
        return getEquipById(equipId, -1);
    }

    private Item getEquipById(int equipId, int ringId)
    {
        Equip nEquip;
        nEquip = new Equip(equipId, 0, ringId);
        nEquip.setQuantity(1);
        var stats = getEquipStats(equipId);
        if (stats != null)
        {
            foreach (var stat in stats)
            {
                var val = (short)stat.Value;
                if (stat.Key.Equals("STR"))
                {
                    nEquip.setStr(val);
                }
                else if (stat.Key.Equals("DEX"))
                {
                    nEquip.setDex(val);
                }
                else if (stat.Key.Equals("INT"))
                {
                    nEquip.setInt(val);
                }
                else if (stat.Key.Equals("LUK"))
                {
                    nEquip.setLuk(val);
                }
                else if (stat.Key.Equals("PAD"))
                {
                    nEquip.setWatk(val);
                }
                else if (stat.Key.Equals("PDD"))
                {
                    nEquip.setWdef(val);
                }
                else if (stat.Key.Equals("MAD"))
                {
                    nEquip.setMatk(val);
                }
                else if (stat.Key.Equals("MDD"))
                {
                    nEquip.setMdef(val);
                }
                else if (stat.Key.Equals("ACC"))
                {
                    nEquip.setAcc(val);
                }
                else if (stat.Key.Equals("EVA"))
                {
                    nEquip.setAvoid(val);
                }
                else if (stat.Key.Equals("Speed"))
                {
                    nEquip.setSpeed(val);
                }
                else if (stat.Key.Equals("Jump"))
                {
                    nEquip.setJump(val);
                }
                else if (stat.Key.Equals("MHP"))
                {
                    nEquip.setHp(val);
                }
                else if (stat.Key.Equals("MMP"))
                {
                    nEquip.setMp(val);
                }
                else if (stat.Key.Equals("tuc"))
                {
                    nEquip.setUpgradeSlots((byte)val);
                }
                else if (isUntradeableRestricted(equipId))
                {  // thanks Hyun & Thora for showing an issue with more than only "Untradeable" items being flagged as such here
                    short flag = nEquip.getFlag();
                    flag |= ItemConstants.UNTRADEABLE;
                    nEquip.setFlag(flag);
                }
                else if (stats.GetValueOrDefault("fs") > 0)
                {
                    short flag = nEquip.getFlag();
                    flag |= ItemConstants.SPIKES;
                    nEquip.setFlag(flag);
                    equipCache.AddOrUpdate(equipId, nEquip);
                }
            }
        }
        return nEquip.copy(); // 为什么要用copy？
    }

    /// <summary>
    /// 范围：[defaultValue - maxRange, defaultValue + maxRange]
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    private static short getRandStat(int defaultValue, int maxRange)
    {
        if (defaultValue == 0)
        {
            return 0;
        }
        int lMaxRange = (int)Math.Min(Math.Ceiling(defaultValue * 0.1), maxRange);
        return (short)(defaultValue - lMaxRange + Math.Floor(Randomizer.nextDouble() * (lMaxRange * 2 + 1)));
    }

    public Equip randomizeStats(Equip equip)
    {
        equip.setStr(getRandStat(equip.getStr(), 5));
        equip.setDex(getRandStat(equip.getDex(), 5));
        equip.setInt(getRandStat(equip.getInt(), 5));
        equip.setLuk(getRandStat(equip.getLuk(), 5));
        equip.setMatk(getRandStat(equip.getMatk(), 5));
        equip.setWatk(getRandStat(equip.getWatk(), 5));
        equip.setAcc(getRandStat(equip.getAcc(), 5));
        equip.setAvoid(getRandStat(equip.getAvoid(), 5));
        equip.setJump(getRandStat(equip.getJump(), 5));
        equip.setSpeed(getRandStat(equip.getSpeed(), 5));
        equip.setWdef(getRandStat(equip.getWdef(), 10));
        equip.setMdef(getRandStat(equip.getMdef(), 10));
        equip.setHp(getRandStat(equip.getHp(), 10));
        equip.setMp(getRandStat(equip.getMp(), 10));
        return equip;
    }

    /// <summary>
    /// 范围：[defaultValue, defaultValue + maxRange]
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    private static short getRandUpgradedStat(int defaultValue, int maxRange)
    {
        if (defaultValue == 0)
        {
            return 0;
        }
        int lMaxRange = maxRange;
        return (short)(defaultValue + (short)Math.Floor(Randomizer.nextDouble() * (lMaxRange + 1)));
    }

    public Equip randomizeUpgradeStats(Equip equip)
    {
        equip.setStr(getRandUpgradedStat(equip.getStr(), 2));
        equip.setDex(getRandUpgradedStat(equip.getDex(), 2));
        equip.setInt(getRandUpgradedStat(equip.getInt(), 2));
        equip.setLuk(getRandUpgradedStat(equip.getLuk(), 2));
        equip.setMatk(getRandUpgradedStat(equip.getMatk(), 2));
        equip.setWatk(getRandUpgradedStat(equip.getWatk(), 2));
        equip.setAcc(getRandUpgradedStat(equip.getAcc(), 2));
        equip.setAvoid(getRandUpgradedStat(equip.getAvoid(), 2));
        equip.setJump(getRandUpgradedStat(equip.getJump(), 2));
        equip.setWdef(getRandUpgradedStat(equip.getWdef(), 5));
        equip.setMdef(getRandUpgradedStat(equip.getMdef(), 5));
        equip.setHp(getRandUpgradedStat(equip.getHp(), 5));
        equip.setMp(getRandUpgradedStat(equip.getMp(), 5));
        return equip;
    }

    public StatEffect? getItemEffect(int itemId)
    {
        var ret = itemEffects.GetValueOrDefault(itemId);
        if (ret == null)
        {
            var item = getItemData(itemId);
            if (item == null)
            {
                return null;
            }
            var spec = item.getChildByPath("specEx") ?? item.getChildByPath("spec");

            ret = StatEffect.loadItemEffectFromData(spec, itemId);
            itemEffects.AddOrUpdate(itemId, ret);
        }
        return ret;
    }

    public StatEffect GetItemEffectTrust(int itemId) => getItemEffect(itemId) ?? throw new BusinessResException($"getItemEffect({itemId})");

    public int[][] getSummonMobs(int itemId)
    {
        var data = getItemData(itemId);
        int theInt = data?.getChildByPath("mob")?.getChildren()?.Count ?? 0;
        int[][] mobs2spawn = new int[theInt][];
        for (int x = 0; x < theInt; x++)
        {
            mobs2spawn[x][0] = DataTool.getIntConvert("mob/" + x + "/id", data);
            mobs2spawn[x][1] = DataTool.getIntConvert("mob/" + x + "/prob", data);
        }
        return mobs2spawn;
    }

    public int getWatkForProjectile(int itemId)
    {
        if (projectileWatkCache.TryGetValue(itemId, out var atk))
            return atk;

        var data = getItemData(itemId);
        atk = DataTool.getInt("info/incPAD", data, 0);
        projectileWatkCache.Add(itemId, atk);
        return atk;
    }

    public string? getName(int itemId)
    {
        if (nameCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        var strings = getStringData(itemId);
        if (strings == null)
        {
            return null;
        }
        var ret = DataTool.getString("name", strings);
        nameCache.Add(itemId, ret);
        return ret;
    }

    public string? getMsg(int itemId)
    {
        if (msgCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        var strings = getStringData(itemId);
        if (strings == null)
        {
            return null;
        }
        var ret = DataTool.getString("msg", strings);
        msgCache.Add(itemId, ret);
        return ret;
    }

    /// <summary>
    /// 交易限制
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isUntradeableRestricted(int itemId)
    {
        if (untradeableCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        bool bRestricted = false;
        if (itemId != 0)
        {
            var data = getItemData(itemId);
            if (data != null)
            {
                bRestricted = DataTool.getIntConvert("info/tradeBlock", data, 0) == 1;
            }
        }

        untradeableCache.Add(itemId, bRestricted);
        return bRestricted;
    }

    public bool isAccountRestricted(int itemId)
    {
        if (accountItemRestrictionCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        bool bRestricted = false;
        if (itemId != 0)
        {
            var data = getItemData(itemId);
            if (data != null)
            {
                bRestricted = DataTool.getIntConvert("info/accountSharable", data, 0) == 1;
            }
        }

        accountItemRestrictionCache.Add(itemId, bRestricted);
        return bRestricted;
    }

    /// <summary>
    /// 丢弃消失（交易限制+账号共享限制）
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private bool isLootRestricted(int itemId)
    {
        if (dropRestrictionCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        bool bRestricted = false;
        if (itemId != 0)
        {
            var data = getItemData(itemId);
            if (data != null)
            {
                bRestricted = DataTool.getIntConvert("info/tradeBlock", data, 0) == 1;
                if (!bRestricted)
                {
                    bRestricted = isAccountRestricted(itemId);
                }
            }
        }

        dropRestrictionCache.Add(itemId, bRestricted);
        return bRestricted;
    }
    /// <summary>
    /// 丢弃限制（不可交易，包含任务道具）
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isDropRestricted(int itemId)
    {
        return isLootRestricted(itemId) || isQuestItem(itemId);
    }

    /// <summary>
    /// 固有道具（只能拥有1个）
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isPickupRestricted(int itemId)
    {
        if (pickupRestrictionCache.TryGetValue(itemId, out var value))
        {
            return value;
        }

        bool bRestricted = false;
        if (itemId != 0)
        {
            var data = getItemData(itemId);
            if (data != null)
            {
                bRestricted = DataTool.getIntConvert("info/only", data, 0) == 1;
            }
        }

        pickupRestrictionCache.Add(itemId, bRestricted);
        return bRestricted;
    }

    private ItemSkillDataPair? getSkillStatsInternal(int itemId)
    {
        var cachedData = SkillUpgreadCache.GetValueOrDefault(itemId);
        if (cachedData != null)
            return cachedData;

        var item = getItemData(itemId);
        if (item != null)
        {
            var info = item.getChildByPath("info");
            if (info != null)
            {
                var ret = new Dictionary<string, int>();

                foreach (Data data in info.getChildren())
                {
                    var dataName = data.getName();
                    if (dataName != null && dataName.StartsWith("inc"))
                    {
                        ret.AddOrUpdate("inc", DataTool.getIntConvert(data));
                    }
                }
                ret.AddOrUpdate("masterLevel", DataTool.getInt("masterLevel", info, 0));
                ret.AddOrUpdate("reqSkillLevel", DataTool.getInt("reqSkillLevel", info, 0));
                ret.AddOrUpdate("success", DataTool.getInt("success", info, 0));

                var retSkill = info.getChildByPath("skill") ?? throw new BusinessResException($"itemId = {itemId}, /info/skill"); ;

                var model = new ItemSkillDataPair(retSkill, ret);
                SkillUpgreadCache.Add(itemId, model);
                return model;
            }
        }
        return null;
    }

    public Dictionary<string, int>? getSkillStats(int itemId, double playerJob)
    {
        var retData = getSkillStatsInternal(itemId);
        if (retData == null)
            return null;

        if (retData.SkillInfo.Count == 0)
        {
            return null;
        }

        Dictionary<string, int> ret = new(retData.SkillInfo);
        var skill = retData.SkillWzData;
        int curskill;
        for (int i = 0; i < skill.getChildren().Count; i++)
        {
            curskill = DataTool.getInt(i.ToString(), skill, 0);
            if (curskill == 0)
            {
                break;
            }
            if (curskill / 10000 == playerJob)
            {
                ret.AddOrUpdate("skillid", curskill);
                break;
            }
        }
        if (!ret.ContainsKey("skillid"))
        {
            ret.AddOrUpdate("skillid", 0);
        }
        return ret;
    }

    public PetCanConsumePair canPetConsume(int petId, int itemId)
    {
        var foodData = cashPetFoodCache.GetValueOrDefault(itemId);
        if (foodData == null)
        {
            HashSet<int> pets = new(4);
            int inc = 1;

            var data = getItemData(itemId);
            if (data != null)
            {
                var specData = data.getChildByPath("spec");
                foreach (Data specItem in specData.getChildren())
                {
                    var itemName = specItem.getName();

                    if (int.TryParse(itemName, out var _))
                    {
                        int petid = DataTool.getInt(specItem, 0);
                        pets.Add(petid);
                    }
                    else
                    {
                        if (itemName == "inc")
                        {
                            inc = DataTool.getInt(specItem, 1);
                        }
                    }
                }
            }

            foodData = new(inc, pets);
            cashPetFoodCache.AddOrUpdate(itemId, foodData);
        }

        return new(foodData.Value.Key, foodData.Value.Value.Contains(petId));
    }
    /// <summary>
    /// 任务道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isQuestItem(int itemId)
    {
        if (isQuestItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        bool questItem = DataTool.getIntConvert("info/quest", getItemData(itemId), 0) == 1;
        isQuestItemCache.Add(itemId, questItem);
        return questItem;
    }

    public bool isPartyQuestItem(int itemId)
    {
        if (isPartyQuestItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        bool partyquestItem = DataTool.getIntConvert("info/pquest", getItemData(itemId), 0) == 1;
        isPartyQuestItemCache.AddOrUpdate(itemId, partyquestItem);
        return partyquestItem;
    }

    private void loadCardIdData()
    {
        monsterBookID = _transport.RequestMonsterCardData().List.ToDictionary(x => x.CardId, x => x.MobId);
    }

    public int getCardMobId(int id)
    {
        return monsterBookID.GetValueOrDefault(id);
    }

    public bool isUntradeableOnEquip(int itemId)
    {
        if (onEquipUntradeableCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        bool untradeableOnEquip = DataTool.getIntConvert("info/equipTradeBlock", getItemData(itemId), 0) > 0;
        onEquipUntradeableCache.AddOrUpdate(itemId, untradeableOnEquip);
        return untradeableOnEquip;
    }

    public ScriptedItem? getScriptedItemInfo(int itemId)
    {
        if (scriptedItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        if (!ItemId.HasScript(itemId))
        {
            return null;
        }
        var itemInfo = getItemData(itemId);
        ScriptedItem script = new ScriptedItem(
                DataTool.getInt("spec/npc", itemInfo, 0),
                DataTool.getString("spec/script", itemInfo) ?? "",
                DataTool.getInt("spec/runOnPickup", itemInfo, 0) == 1);
        scriptedItemCache.AddOrUpdate(itemId, script);
        return scriptedItemCache[itemId];
    }

    public bool isKarmaAble(int itemId)
    {
        if (karmaCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        bool bRestricted = DataTool.getIntConvert("info/tradeAvailable", getItemData(itemId), 0) > 0;
        karmaCache.Add(itemId, bRestricted);
        return bRestricted;
    }

    public int getStateChangeItem(int itemId)
    {
        if (triggerItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int triggerItem = DataTool.getIntConvert("info/stateChangeItem", getItemData(itemId), 0);
            triggerItemCache.Add(itemId, triggerItem);
            return triggerItem;
        }
    }

    public int getCreateItem(int itemId)
    {
        if (createItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int itemFrom = DataTool.getIntConvert("info/create", getItemData(itemId), 0);
            createItemCache.Add(itemId, itemFrom);
            return itemFrom;
        }
    }

    public int getMobItem(int itemId)
    {
        if (mobItemCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int mobItemCatch = DataTool.getIntConvert("info/mob", getItemData(itemId), 0);
            mobItemCache.Add(itemId, mobItemCatch);
            return mobItemCatch;
        }
    }

    public int getUseDelay(int itemId)
    {
        if (useDelayCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int mobUseDelay = DataTool.getIntConvert("info/useDelay", getItemData(itemId), 0);
            useDelayCache.Add(itemId, mobUseDelay);
            return mobUseDelay;
        }
    }

    public int getMobHP(int itemId)
    {
        if (mobHPCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int mobHPItem = DataTool.getIntConvert("info/mobHP", getItemData(itemId), 0);
            mobHPCache.Add(itemId, mobHPItem);
            return mobHPItem;
        }
    }

    public int getExpById(int itemId)
    {
        if (expCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int exp = DataTool.getIntConvert("spec/exp", getItemData(itemId), 0);
            expCache.Add(itemId, exp);
            return exp;
        }
    }

    public int getMaxLevelById(int itemId)
    {
        if (levelCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        else
        {
            int level = DataTool.getIntConvert("info/maxLevel", getItemData(itemId), 256);
            levelCache.Add(itemId, level);
            return level;
        }
    }

    public KeyValuePair<int, List<RewardItem>> getItemReward(int itemId)
    {
        //Thanks Celino - used some stuffs :)
        if (rewardCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        int totalprob = 0;
        List<RewardItem> rewards = new();
        foreach (Data child in getItemData(itemId).getChildByPath("reward").getChildren())
        {
            RewardItem reward = new RewardItem();
            reward.itemid = DataTool.getInt("item", child, 0);
            reward.prob = (byte)DataTool.getInt("prob", child, 0);
            reward.quantity = (short)DataTool.getInt("count", child, 0);
            reward.effect = DataTool.getString("Effect", child) ?? "";
            reward.worldmsg = DataTool.getString("worldMsg", child);
            reward.period = DataTool.getInt("period", child, -1);

            totalprob += reward.prob;

            rewards.Add(reward);
        }
        KeyValuePair<int, List<RewardItem>> hmm = new(totalprob, rewards);
        rewardCache.Add(itemId, hmm);
        return hmm;
    }

    public bool isConsumeOnPickup(int itemId)
    {
        if (consumeOnPickupCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        var data = getItemData(itemId);
        bool consume = DataTool.getIntConvert("spec/consumeOnPickup", data, 0) == 1 || DataTool.getIntConvert("specEx/consumeOnPickup", data, 0) == 1;
        consumeOnPickupCache.Add(itemId, consume);
        return consume;
    }

    public bool isTwoHanded(int itemId)
    {
        var arr = new WeaponType[] {
            WeaponType.GENERAL2H_SWING ,
            WeaponType.BOW ,
            WeaponType.CLAW,
            WeaponType.CROSSBOW,
            WeaponType.POLE_ARM_SWING,
            WeaponType.SPEAR_STAB,
            WeaponType.SWORD2H,
            WeaponType.GUN,
            WeaponType.KNUCKLE};
        return arr.Contains(getWeaponType(itemId));
    }

    public bool isCash(int itemId)
    {
        int itemType = itemId / 1000000;
        if (itemType == 5)
        {
            return true;
        }
        if (itemType != 1)
        {
            return false;
        }

        var eqpStats = getEquipStats(itemId);
        return eqpStats != null && eqpStats.GetValueOrDefault("cash") == 1;
    }

    public bool isUpgradeable(int itemId)
    {
        Item it = getEquipById(itemId);
        Equip eq = (Equip)it;

        return eq.getUpgradeSlots() > 0 || eq.getStr() > 0 || eq.getDex() > 0 || eq.getInt() > 0 || eq.getLuk() > 0 ||
                eq.getWatk() > 0 || eq.getMatk() > 0 || eq.getWdef() > 0 || eq.getMdef() > 0 || eq.getAcc() > 0 ||
                eq.getAvoid() > 0 || eq.getSpeed() > 0 || eq.getJump() > 0 || eq.getHp() > 0 || eq.getMp() > 0;
    }

    public bool isUnmerchable(int itemId)
    {
        if (YamlConfig.config.server.USE_ENFORCE_UNMERCHABLE_CASH && isCash(itemId))
        {
            return true;
        }

        return YamlConfig.config.server.USE_ENFORCE_UNMERCHABLE_PET && ItemConstants.isPet(itemId);
    }

    public ICollection<Item> canWearEquipment(IPlayer chr, ICollection<Item> items)
    {
        Inventory inv = chr.getInventory(InventoryType.EQUIPPED);
        if (inv.IsChecked())
        {
            return items;
        }
        List<Item> itemz = new();
        if (chr.getJob() == Job.SUPERGM || chr.getJob() == Job.GM)
        {
            foreach (Item item in items)
            {
                Equip equip = (Equip)item;
                equip.wear(true);
                itemz.Add(item);
            }
            return itemz;
        }
        bool highfivestamp = false;
        /* Removed because players shouldn't even get this, and gm's should just be gm job.
         try {
         foreach(Pair<Item, InventoryType> ii in ItemFactory.INVENTORY.loadItems(chr.getId(), false)) {
         if (ii.getRight() == InventoryType.CASH) {
         if (ii.getLeft().getItemId() == 5590000) {
         highfivestamp = true;
         }
         }
         }
         } catch (SQLException ex) {
            Log.Logger.Error(ex.ToString());
         }*/
        int tdex = chr.getDex(), tstr = chr.getStr(), tint = chr.getInt(), tluk = chr.getLuk(), fame = chr.getFame();
        // 是不是有问题？
        if (chr.getJob() != Job.SUPERGM || chr.getJob() != Job.GM)
        {
            foreach (Item item in inv.list())
            {
                Equip equip = (Equip)item;
                tdex += equip.getDex();
                tstr += equip.getStr();
                tluk += equip.getLuk();
                tint += equip.getInt();
            }
        }
        foreach (Item item in items)
        {
            Equip equip = (Equip)item;
            int reqLevel = getEquipLevelReq(equip.getItemId());
            if (highfivestamp)
            {
                reqLevel -= 5;
                if (reqLevel < 0)
                {
                    reqLevel = 0;
                }
            }
            var equipState = getEquipStats(equip.getItemId());
            /*
             int reqJob = getEquipStats(equip.getItemId()).get("reqJob");
             if (reqJob != 0) {
             Really hard check, and not really needed in this one
             Gm's should just be GM job, and players cannot change jobs.
             }*/
            if (reqLevel > chr.getLevel())
            {
                continue;
            }
            else if (equipState?.GetValueOrDefault("reqDEX") > tdex)
            {
                continue;
            }
            else if (equipState?.GetValueOrDefault("reqSTR") > tstr)
            {
                continue;
            }
            else if (equipState?.GetValueOrDefault("reqLUK") > tluk)
            {
                continue;
            }
            else if (equipState?.GetValueOrDefault("reqINT") > tint)
            {
                continue;
            }
            var reqPOP = equipState?.GetValueOrDefault("reqPOP");
            if (reqPOP > 0)
            {
                if (reqPOP > fame)
                {
                    continue;
                }
            }
            equip.wear(true);
            itemz.Add(equip);
        }
        inv.SetChecked(true);
        return itemz;
    }

    public bool canWearEquipment(IPlayer chr, Equip equip, int dst)
    {
        int id = equip.getItemId();

        if (ItemId.isWeddingRing(id) && chr.hasJustMarried())
        {
            chr.dropMessage(5, "The Wedding Ring cannot be equipped on this map.");  // will dc everyone due to doubled couple effect
            return false;
        }

        var islot = getEquipmentSlot(id);
        if (!EquipSlot.getFromTextSlot(islot).isAllowed(dst, isCash(id)))
        {
            equip.wear(false);
            var itemName = getInstance().getName(equip.getItemId());
            chr.Client.CurrentServerContainer.BroadcastWorldGMPacket(PacketCreator.sendYellowTip("[Warning]: " + chr.getName() + " tried to equip " + itemName + " into slot " + dst + "."));
            AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to forcibly equip an item.");
            _logger.LogWarning("Chr {CharacterName} tried to equip {ItemName} into slot {Slot}", chr.getName(), itemName, dst);
            return false;
        }

        if (chr.getJob() == Job.SUPERGM || chr.getJob() == Job.GM)
        {
            equip.wear(true);
            return true;
        }


        bool highfivestamp = chr.Bag[InventoryType.CASH].HasItem(5590000);

        int reqLevel = getEquipLevelReq(equip.getItemId());
        if (highfivestamp)
        {
            reqLevel -= 5;
        }
        int i = 0; //lol xD
                   //Removed job check. Shouldn't really be needed.

        var equipState = getEquipStats(equip.getItemId());
        if (reqLevel > chr.getLevel())
        {
            i++;
        }
        else if (equipState?.GetValueOrDefault("reqDEX") > chr.getTotalDex())
        {
            i++;
        }
        else if (equipState?.GetValueOrDefault("reqSTR") > chr.getTotalStr())
        {
            i++;
        }
        else if (equipState?.GetValueOrDefault("reqLUK") > chr.getTotalLuk())
        {
            i++;
        }
        else if (equipState?.GetValueOrDefault("reqINT") > chr.getTotalInt())
        {
            i++;
        }
        var reqPOP = equipState?.GetValueOrDefault("reqPOP");
        if (reqPOP > 0)
        {
            if (reqPOP > chr.getFame())
            {
                i++;
            }
        }

        if (i > 0)
        {
            equip.wear(false);
            return false;
        }
        equip.wear(true);
        return true;
    }

    public List<ItemInfoBase> getItemDataByName(string name)
    {
        return getInstance().getAllItems().Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private Data? getEquipLevelInfo(int itemId)
    {
        if (equipLevelInfoCache.TryGetValue(itemId, out var value) && value != null)
            return value;

        var iData = getItemData(itemId);
        if (iData != null)
        {
            var data = iData.getChildByPath("info/level");
            if (data != null)
            {
                value = data.getChildByPath("info");
            }
        }

        equipLevelInfoCache.AddOrUpdate(itemId, value);
        return value;
    }

    public int getEquipLevel(int itemId, bool getMaxLevel)
    {
        if (!equipMaxLevelCache.TryGetValue(itemId, out var eqLevel))
        {
            eqLevel = 1;    // greater than 1 means that it was supposed to levelup on GMS

            var data = getEquipLevelInfo(itemId);
            if (data != null)
            {
                if (getMaxLevel)
                {
                    int curLevel = 1;

                    while (true)
                    {
                        var data2 = data.getChildByPath(curLevel.ToString());
                        if (data2 == null || data2.getChildren().Count <= 1)
                        {
                            eqLevel = curLevel;
                            equipMaxLevelCache.Add(itemId, eqLevel);
                            break;
                        }

                        curLevel++;
                    }
                }
                else
                {
                    var data2 = data.getChildByPath("1");
                    if (data2 != null && data2.getChildren().Count > 1)
                    {
                        eqLevel = 2;
                    }
                }
            }
        }

        return eqLevel;
    }

    public List<KeyValuePair<string, int>> getItemLevelupStats(int itemId, int level)
    {
        List<KeyValuePair<string, int>> list = new();
        var data = getEquipLevelInfo(itemId);
        if (data != null)
        {
            var data2 = data.getChildByPath(level.ToString());
            if (data2 != null)
            {
                foreach (Data da in data2.getChildren())
                {
                    if (Randomizer.nextDouble() < 0.9)
                    {
                        var dataName = da.getName() ?? "";
                        if (dataName.StartsWith("incDEXMin"))
                        {
                            list.Add(new("incDEX", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incDEXMax")))));
                        }
                        else if (dataName.StartsWith("incSTRMin"))
                        {
                            list.Add(new("incSTR", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incSTRMax")))));
                        }
                        else if (dataName.StartsWith("incINTMin"))
                        {
                            list.Add(new("incINT", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incINTMax")))));
                        }
                        else if (dataName.StartsWith("incLUKMin"))
                        {
                            list.Add(new("incLUK", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incLUKMax")))));
                        }
                        else if (dataName.StartsWith("incMHPMin"))
                        {
                            list.Add(new("incMHP", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incMHPMax")))));
                        }
                        else if (dataName.StartsWith("incMMPMin"))
                        {
                            list.Add(new("incMMP", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incMMPMax")))));
                        }
                        else if (dataName.StartsWith("incPADMin"))
                        {
                            list.Add(new("incPAD", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incPADMax")))));
                        }
                        else if (dataName.StartsWith("incMADMin"))
                        {
                            list.Add(new("incMAD", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incMADMax")))));
                        }
                        else if (dataName.StartsWith("incPDDMin"))
                        {
                            list.Add(new("incPDD", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incPDDMax")))));
                        }
                        else if (dataName.StartsWith("incMDDMin"))
                        {
                            list.Add(new("incMDD", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incMDDMax")))));
                        }
                        else if (dataName.StartsWith("incACCMin"))
                        {
                            list.Add(new("incACC", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incACCMax")))));
                        }
                        else if (dataName.StartsWith("incEVAMin"))
                        {
                            list.Add(new("incEVA", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incEVAMax")))));
                        }
                        else if (dataName.StartsWith("incSpeedMin"))
                        {
                            list.Add(new("incSpeed", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incSpeedMax")))));
                        }
                        else if (dataName.StartsWith("incJumpMin"))
                        {
                            list.Add(new("incJump", Randomizer.rand(DataTool.getInt(da), DataTool.getInt(data2.getChildByPath("incJumpMax")))));
                        }
                    }
                }
            }
        }

        return list;
    }

    private bool canUseSkillBook(IPlayer player, int skillBookId)
    {
        var skilldata = getSkillStats(skillBookId, player.getJob().getId());
        if (skilldata == null || skilldata.GetValueOrDefault("skillid") == 0)
        {
            return false;
        }

        var skill2 = SkillFactory.getSkill(skilldata.GetValueOrDefault("skillid"));
        return (player.getSkillLevel(skill2) >= skilldata.GetValueOrDefault("reqSkillLevel") || skilldata.GetValueOrDefault("reqSkillLevel") == 0)
            && player.getMasterLevel(skill2) < skilldata.GetValueOrDefault("masterLevel");
    }

    public List<int> usableMasteryBooks(IPlayer player)
    {
        List<int> masterybook = new();
        for (int i = 2290000; i <= 2290139; i++)
        {
            if (canUseSkillBook(player, i))
            {
                masterybook.Add(i);
            }
        }

        return masterybook;
    }

    public List<int> usableSkillBooks(IPlayer player)
    {
        List<int> skillbook = new();
        for (int i = 2280000; i <= 2280019; i++)
        {
            if (canUseSkillBook(player, i))
            {
                skillbook.Add(i);
            }
        }

        return skillbook;
    }

    public QuestConsItem? getQuestConsumablesInfo(int itemId)
    {
        if (questItemConsCache.TryGetValue(itemId, out var value))
        {
            return value;
        }
        var data = getItemData(itemId);
        QuestConsItem? qcItem = null;

        var infoData = data?.getChildByPath("info");
        if (infoData?.getChildByPath("uiData") != null)
        {
            qcItem = new QuestConsItem();
            qcItem.exp = DataTool.getInt("exp", infoData);
            qcItem.grade = DataTool.getInt("grade", infoData);
            qcItem.questid = DataTool.getInt("questId", infoData);
            qcItem.items = new(2);

            Dictionary<int, int> cItems = qcItem.items;
            var ciData = infoData.getChildByPath("consumeItem");
            if (ciData != null)
            {
                foreach (Data ciItem in ciData.getChildren())
                {
                    int itemid = DataTool.getInt("0", ciItem);
                    int qty = DataTool.getInt("1", ciItem);

                    cItems.AddOrUpdate(itemid, qty);
                }
            }
        }

        questItemConsCache.AddOrUpdate(itemId, qcItem);
        return qcItem;
    }

    public class ScriptedItem
    {

        private bool _runOnPickup;
        private int npc;
        private string script;

        public ScriptedItem(int npc, string script, bool rop)
        {
            this.npc = npc;
            this.script = script;
            _runOnPickup = rop;
        }

        public int getNpc()
        {
            return npc;
        }

        public string getScript()
        {
            return script;
        }

        public bool runOnPickup()
        {
            return _runOnPickup;
        }
    }

    public class RewardItem
    {

        public int itemid, period;
        public short prob, quantity;
        public string? effect, worldmsg;
    }

    public class QuestConsItem
    {

        public int questid, exp, grade;
        public Dictionary<int, int> items = [];

        public int getItemRequirement(int itemid)
        {
            return items.GetValueOrDefault(itemid);
        }

    }
}

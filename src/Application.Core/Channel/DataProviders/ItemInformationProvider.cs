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


using Application.Core.Channel.ServerData;
using Application.Core.Game.Items;
using Application.Core.Game.Skills;
using Application.Resources;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Templates;
using Application.Templates.Character;
using Application.Templates.Exceptions;
using Application.Templates.Item;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Pet;
using Application.Templates.Providers;
using Application.Templates.StatEffectProps;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Impl.AdoJobStore.Common;
using server;
using tools;

namespace Application.Core.Channel.DataProviders;


/**
 * @author Matze
 */
public class ItemInformationProvider : DataBootstrap, IStaticService
{
    private static ItemInformationProvider? _instance;

    public static ItemInformationProvider getInstance()
    {
        return _instance ?? throw new BusinessFatalException("ItemInformationProvider 未注册");
    }



    readonly AutoBanDataManager _autoBanDataManager;

    readonly EquipProvider _equipProvider = ProviderSource.Instance.GetProvider<EquipProvider>();
    readonly ItemProvider _itemProvider = ProviderSource.Instance.GetProvider<ItemProvider>();
    public ItemInformationProvider(
        ILogger<DataBootstrap> logger,
        AutoBanDataManager autoBanDataManager) : base(logger)
    {
        Name = "物品数据";


        _autoBanDataManager = autoBanDataManager;
    }

    public void Register(IServiceProvider sp)
    {
        _instance = sp.GetService<ItemInformationProvider>() ?? throw new BusinessFatalException("ItemInformationProvider 未注册");
    }

    public void Register(ItemInformationProvider instance)
    {
        _instance = instance;
    }

    protected override void LoadDataInternal()
    {
        loadCardIdData();
    }


    protected Dictionary<int, StatEffect> itemEffects = new();
    protected Dictionary<int, int> monsterBookID = new();

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
    AbstractGroupProvider<AbstractItemTemplate> GetProvider(int itemId)
    {
        return itemId < 2000000 ? _equipProvider : _itemProvider;
    }

    public AbstractItemTemplate? GetTemplate(int itemId) => GetProvider(itemId).GetItem(itemId);
    public AbstractItemTemplate GetTrustTemplate(int itemId)
    {
        var provider = GetProvider(itemId);
        return provider.GetItem(itemId) ?? throw new TemplateNotFoundException(provider.ProviderName, itemId);
    }
    public EquipTemplate? GetEquipTemplate(int equipId) => _equipProvider.GetRequiredItem<EquipTemplate>(equipId);
    public bool noCancelMouse(int itemId)
    {
        return GetProvider(itemId).GetRequiredItem<ConsumeItemTemplate>(itemId)?.NoCancelMouse ?? false;
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
        return (short)((GetProvider(itemId).GetItem(itemId)?.SlotMax ?? 1) + getExtraSlotMaxFromPlayer(c, itemId));
    }

    public MesoBagItemTemplate? GetMesoBagItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<MesoBagItemTemplate>(itemId);



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

    //private KeyValuePair<int, double> getItemPriceData(int itemId)
    //{
    //    var item = getItemData(itemId);
    //    if (item == null)
    //    {
    //        wholePriceCache.AddOrUpdate(itemId, -1);
    //        unitPriceCache.AddOrUpdate(itemId, 0.0);
    //        return new(-1, 0.0);
    //    }

    //    int pEntry = -1;
    //    var pData = item.getChildByPath("info/price");
    //    if (pData != null)
    //    {
    //        pEntry = DataTool.getInt(pData);
    //    }

    //    double fEntry = 0.0f;
    //    pData = item.getChildByPath("info/unitPrice");
    //    if (pData != null)
    //    {
    //        try
    //        {
    //            fEntry = getRoundedUnitPrice(DataTool.getDouble(pData), 5);
    //        }
    //        catch (Exception)
    //        {
    //            fEntry = DataTool.getInt(pData);
    //        }
    //    }

    //    wholePriceCache.AddOrUpdate(itemId, pEntry);
    //    unitPriceCache.AddOrUpdate(itemId, fEntry);
    //    return new(pEntry, fEntry);
    //}

    public int getWholePrice(int itemId)
    {
        return GetProvider(itemId).GetItem(itemId)?.Price ?? -1;
    }

    public double getUnitPrice(int itemId)
    {
        return getRoundedUnitPrice(GetProvider(itemId).GetRequiredItem<BulletItemTemplate>(itemId)?.UnitPrice ?? 0, 5);
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

    public ReplaceItemTemplate? GetReplaceItemTemplate(int itemId) => GetProvider(itemId).GetItem(itemId)?.ReplaceItem;

    public int getEquipLevelReq(int itemId)
    {
        return GetEquipTemplate(itemId)?.ReqLevel ?? 0;
    }

    public int[] getScrollReqs(int itemId)
    {
        return GetProvider(itemId).GetRequiredItem<ScrollItemTemplate>(itemId)?.Req ?? [];
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
        int totalUpgradeCount = equip.SourceTemplate.TUC;
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
    public Equip scrollEquipWithId(Equip nEquip, int scrollId, bool usingWhiteScroll, int vegaItemId, bool isGM)
    {
        bool assertGM = isGM && YamlConfig.config.server.USE_PERFECT_GM_SCROLL;

        var scrollTemplate = _itemProvider.GetRequiredItem<ScrollItemTemplate>(scrollId);
        if (scrollTemplate == null)
            return nEquip;

        if (nEquip.getUpgradeSlots() > 0 || ItemConstants.isCleanSlate(scrollId) || assertGM)
        {
            double prop = scrollTemplate.SuccessRate;

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
                if (scrollTemplate.PreventSlip)
                {
                    flag |= ItemConstants.SPIKES;
                    nEquip.setFlag((byte)flag);
                }
                if (scrollTemplate.PreventSlip)
                {
                    flag |= ItemConstants.COLD;
                    nEquip.setFlag((byte)flag);
                }
                if (scrollTemplate.Recover)
                {
                    if (canUseCleanSlate(nEquip))
                    {
                        nEquip.setUpgradeSlots(nEquip.getUpgradeSlots() + 1);
                    }
                }
                if (scrollTemplate.RandStat)
                {
                    scrollEquipWithChaos(nEquip, YamlConfig.config.server.CHSCROLL_STAT_RANGE);
                }
                else
                {
                    improveEquipStats(nEquip, scrollTemplate);
                }
                if (!ItemConstants.isCleanSlate(scrollId))
                {
                    if (!assertGM && !ItemConstants.isModifierScroll(scrollId))
                    {   // issue with modifier scrolls taking slots found thanks to Masterrulax, justin, BakaKnyx
                        nEquip.setUpgradeSlots(nEquip.getUpgradeSlots() - 1);
                    }
                    nEquip.setLevel((byte)(nEquip.getLevel() + 1));
                }
            }
            else
            {
                if (!YamlConfig.config.server.USE_PERFECT_SCROLLING && !usingWhiteScroll && !ItemConstants.isCleanSlate(scrollId) && !assertGM && !ItemConstants.isModifierScroll(scrollId))
                {
                    nEquip.setUpgradeSlots(nEquip.getUpgradeSlots() - 1);
                }
                if (Randomizer.nextInt(100) < scrollTemplate.CursedRate)
                {
                    return null;
                }
            }
        }
        return nEquip;
    }

    public static void improveEquipStats(Equip nEquip, ScrollItemTemplate scrollTemplate)
    {
        nEquip.setStr(getShortMaxIfOverflow(nEquip.getStr() + scrollTemplate.IncSTR));
        nEquip.setDex(getShortMaxIfOverflow(nEquip.getDex() + scrollTemplate.IncDEX));
        nEquip.setInt(getShortMaxIfOverflow(nEquip.getInt() + scrollTemplate.IncINT));
        nEquip.setLuk(getShortMaxIfOverflow(nEquip.getLuk() + scrollTemplate.IncLUK));

        nEquip.setWatk(getShortMaxIfOverflow(nEquip.getWatk() + scrollTemplate.IncPAD));
        nEquip.setWdef(getShortMaxIfOverflow(nEquip.getWdef() + scrollTemplate.IncPDD));
        nEquip.setMatk(getShortMaxIfOverflow(nEquip.getMatk() + scrollTemplate.IncMAD));
        nEquip.setMdef(getShortMaxIfOverflow(nEquip.getMdef() + scrollTemplate.IncMDD));

        nEquip.setAcc(getShortMaxIfOverflow(nEquip.getAcc() + scrollTemplate.IncACC));
        nEquip.setAvoid(getShortMaxIfOverflow(nEquip.getAvoid() + scrollTemplate.IncEVA));

        nEquip.setSpeed(getShortMaxIfOverflow(nEquip.getSpeed() + scrollTemplate.IncSpeed));
        nEquip.setJump(getShortMaxIfOverflow(nEquip.getJump() + scrollTemplate.IncJump));
        nEquip.setHp(getShortMaxIfOverflow(nEquip.getHp() + scrollTemplate.IncMHP));
        nEquip.setMp(getShortMaxIfOverflow(nEquip.getMp() + scrollTemplate.IncMMP));
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

    public Equip? GetEquipByTemplate(EquipTemplate? equipTemplate)
    {
        if (equipTemplate == null)
            return null;

        var nEquip = new Equip(equipTemplate, 0);
        nEquip.setQuantity(1);

        nEquip.setStr(equipTemplate.IncSTR);
        nEquip.setDex(equipTemplate.IncDEX);
        nEquip.setInt(equipTemplate.IncINT);
        nEquip.setLuk(equipTemplate.IncLUK);

        nEquip.setWatk(equipTemplate.IncPAD);
        nEquip.setWdef(equipTemplate.IncPDD);
        nEquip.setMatk(equipTemplate.IncMAD);
        nEquip.setMdef(equipTemplate.IncMDD);

        nEquip.setAcc(equipTemplate.IncACC);
        nEquip.setAvoid(equipTemplate.IncEVA);

        nEquip.setSpeed(equipTemplate.IncSpeed);
        nEquip.setJump(equipTemplate.IncJump);
        nEquip.setHp(equipTemplate.IncMHP);
        nEquip.setMp(equipTemplate.IncMMP);
        nEquip.setUpgradeSlots(equipTemplate.TUC);

        if (equipTemplate.TradeBlock)
        {  // thanks Hyun & Thora for showing an issue with more than only "Untradeable" items being flagged as such here
            short flag = nEquip.getFlag();
            flag |= ItemConstants.UNTRADEABLE;
            nEquip.setFlag(flag);
        }
        if (equipTemplate.Fs > 0)
        {
            short flag = nEquip.getFlag();
            flag |= ItemConstants.SPIKES;
            nEquip.setFlag(flag);
        }
        return nEquip;
        //return nEquip.copy(); // Q.为什么要用copy？
    }

    public Item? GenerateVirtualItemById(int itemId, int quantity, bool randomizeEquip = true)
    {
        if (quantity <= 0)
            return null;

        var abTemplate = GetTemplate(itemId);
        if (abTemplate == null)
            return null;

        if (abTemplate is EquipTemplate equipTemplate)
        {
            var item = GetEquipByTemplate(equipTemplate);
            if (item != null)
            {
                if (randomizeEquip)
                {
                    randomizeStats(item);
                }
                return item;
            }
        }

        else
        {
            return Item.CreateVirtualItem(itemId, (short)quantity);
        }

        return null;

    }

    public Equip getEquipById(int equipId)
    {
        return GetEquipByTemplate(GetEquipTemplate(equipId)) ?? throw new TemplateNotFoundException(_equipProvider.ProviderName, equipId);
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
        if (itemEffects.TryGetValue(itemId, out var data))
            return data;

        var item = GetProvider(itemId).GetItem(itemId) as IItemStatEffectProp;
        if (item == null)
            return null;
        return itemEffects[itemId] = new StatEffect(item, item, false);
    }

    public StatEffect GetItemEffectTrust(int itemId) => getItemEffect(itemId) ?? throw new BusinessResException($"getItemEffect({itemId})");

    public SummonMobItemTemplate? GetSummonMobItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<SummonMobItemTemplate>(itemId);

    public int getWatkForProjectile(int itemId)
    {
        return GetProvider(itemId).GetRequiredItem<BulletItemTemplate>(itemId)?.IncPAD ?? 0;
    }

    public bool HasTemplate(int itemId)
    {
        return GetProvider(itemId).GetItem(itemId) != null;
    }
    public string? getName(int itemId)
    {
        return ClientCulture.SystemCulture.GetItemName(itemId);
    }

    public bool IsValidEquip(int itemId, EquipSlot equipType)
    {
        var template = GetEquipTemplate(itemId);
        if (template == null)
            return false;

        return template.Islot == equipType.getName();
    }
    public bool IsHair(int itemId) => IsValidEquip(itemId, EquipSlot.Hair);

    public bool IsFace(int itemId) => IsValidEquip(itemId, EquipSlot.Face);

    /// <summary>
    /// 交易限制
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isUntradeableRestricted(int itemId)
    {
        return GetProvider(itemId).GetItem(itemId)?.TradeBlock ?? false;
    }

    public bool isAccountRestricted(int itemId)
    {
        return GetProvider(itemId).GetItem(itemId)?.AccountSharable ?? false;
    }

    /// <summary>
    /// 丢弃消失（交易限制+账号共享限制）
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private bool isLootRestricted(int itemId)
    {
        var item = GetProvider(itemId).GetItem(itemId);
        if (item == null)
            return false;
        return item.TradeBlock || item.AccountSharable;
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
        return GetProvider(itemId).GetItem(itemId)?.Only ?? false;
    }

    public MasteryItemTemplate? GetMasteryItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<MasteryItemTemplate>(itemId);

    public CashPetFoodItemTemplate? GetCashPetFoodTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<CashPetFoodItemTemplate>(itemId);

    /// <summary>
    /// 任务道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool isQuestItem(int itemId)
    {
        if (itemId <= 0)
            return false;
        return GetProvider(itemId).GetItem(itemId)?.Quest ?? false;
    }

    public bool isPartyQuestItem(int itemId)
    {
        return GetProvider(itemId).GetRequiredItem<ItemTemplateBase>(itemId)?.PartyQuest ?? false;
    }

    private void loadCardIdData()
    {
        monsterBookID = _itemProvider.GetAllMonsterCard().ToDictionary(x => x.TemplateId, x => x.MobId);
    }

    public int getCardMobId(int id)
    {
        return monsterBookID.GetValueOrDefault(id);
    }

    public int[] getCardTierSize()
    {
        return monsterBookID.Keys.GroupBy(x => (int)(Math.Floor(x / 1000d))).Select(x => x.Count()).ToArray();
    }

    public ScriptItemTemplate? GetScriptItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<ScriptItemTemplate>(itemId);

    public bool isKarmaAble(int itemId)
    {
        return GetProvider(itemId).GetItem(itemId)?.TradeAvailable ?? false;
    }

    public ConsumeItemTemplate? GetConsumeItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<ConsumeItemTemplate>(itemId);

    public int getStateChangeItem(int itemId)
    {
        return GetProvider(itemId).GetRequiredItem<MapBuffItemTemplate>(itemId)?.StateChangeItem ?? 0;
    }
    public CatchMobItemTemplate? GetCatchMobItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<CatchMobItemTemplate>(itemId);
    public SolomenItemTemplate? GetSolomenItemTemplate(int itemId) => GetProvider(itemId).GetRequiredItem<SolomenItemTemplate>(itemId);

    public RewardData[] GetItemRewardData(int itemId)
    {
        var data = GetProvider(itemId).GetItem(itemId) as IStatEffectReward;
        if (data == null)
            return [];

        return data.Reward;
    }
    public KeyValuePair<int, List<RewardItem>> getItemReward(int itemId)
    {
        var data = GetProvider(itemId).GetItem(itemId) as IStatEffectReward;
        if (data == null)
            return new KeyValuePair<int, List<RewardItem>>(0, []);

        int totalprob = 0;
        List<RewardItem> rewards = new();
        foreach (var child in data.Reward)
        {
            RewardItem reward = new RewardItem()
            {
                effect = child.Effect,
                itemid = child.ItemID,
                period = child.Period,
                prob = (short)child.Prob,
                quantity = (short)child.Count,
                worldmsg = child.WorldMessage
            };
            totalprob += reward.prob;

            rewards.Add(reward);
        }
        return new(totalprob, rewards);
    }

    public bool isConsumeOnPickup(int itemId)
    {
        var item = GetProvider(itemId).GetRequiredItem<ConsumeItemTemplate>(itemId);
        if (item == null)
            return false;

        return item.ConsumeOnPickup || item.ConsumeOnPickupEx;
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

    public PetItemTemplate? GetPetTemplate(int itemId) => _itemProvider.GetRequiredItem<PetItemTemplate>(itemId);

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

        return GetProvider(itemId).GetItem(itemId)?.Cash ?? false;
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

            int reqLevel = equip.SourceTemplate.ReqLevel;
            if (highfivestamp)
            {
                reqLevel -= 5;
                if (reqLevel < 0)
                {
                    reqLevel = 0;
                }
            }
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
            else if (equip.SourceTemplate.ReqDEX > tdex)
            {
                continue;
            }
            else if (equip.SourceTemplate.ReqSTR > tstr)
            {
                continue;
            }
            else if (equip.SourceTemplate.ReqLUK > tluk)
            {
                continue;
            }
            else if (equip.SourceTemplate.ReqINT > tint)
            {
                continue;
            }
            var reqPOP = equip.SourceTemplate.ReqPOP;
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

        if (!EquipSlot.getFromTextSlot(equip.SourceTemplate.Islot).isAllowed(dst, isCash(id)))
        {
            equip.wear(false);
            var itemName = chr.Client.CurrentCulture.GetItemName(equip.getItemId());

            chr.Client.CurrentServerContainer.SendYellowTip("[Warning]: " + chr.getName() + " tried to equip " + itemName + " into slot " + dst + ".", true);
            _autoBanDataManager.Alert(AutobanFactory.PACKET_EDIT, chr, chr.getName() + " tried to forcibly equip an item.");
            _logger.LogWarning("Chr {CharacterName} tried to equip {ItemName} into slot {Slot}", chr.getName(), itemName, dst);
            return false;
        }

        if (chr.getJob() == Job.SUPERGM || chr.getJob() == Job.GM)
        {
            equip.wear(true);
            return true;
        }


        bool highfivestamp = chr.Bag[InventoryType.CASH].HasItem(5590000);

        int reqLevel = equip.SourceTemplate.ReqLevel;
        if (highfivestamp)
        {
            reqLevel -= 5;
        }
        int i = 0; //lol xD
                   //Removed job check. Shouldn't really be needed.
        if (reqLevel > chr.getLevel())
        {
            i++;
        }
        else if (equip.SourceTemplate.ReqDEX > chr.getTotalDex())
        {
            i++;
        }
        else if (equip.SourceTemplate.ReqSTR > chr.getTotalStr())
        {
            i++;
        }
        else if (equip.SourceTemplate.ReqLUK > chr.getTotalLuk())
        {
            i++;
        }
        else if (equip.SourceTemplate.ReqINT > chr.getTotalInt())
        {
            i++;
        }
        var reqPOP = equip.SourceTemplate.ReqPOP;
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

    public List<KeyValuePair<string, int>> getItemLevelupStats(EquipTemplate template, int level)
    {
        List<KeyValuePair<string, int>> list = new();
        var levelData = template.LevelData.FirstOrDefault(x => x.Level == level);
        if (levelData != null)
        {
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incDEX", Randomizer.rand(levelData.IncDEXMin, levelData.IncDEXMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incSTR", Randomizer.rand(levelData.IncSTRMin, levelData.IncSTRMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incINT", Randomizer.rand(levelData.IncINTMin, levelData.IncINTMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incLUK", Randomizer.rand(levelData.IncLUKMin, levelData.IncLUKMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incMHP", Randomizer.rand(levelData.IncMHPMin, levelData.IncMHPMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incMMP", Randomizer.rand(levelData.IncMMPMin, levelData.IncMMPMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incPAD", Randomizer.rand(levelData.IncPADMin, levelData.IncPADMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incMAD", Randomizer.rand(levelData.IncMADMin, levelData.IncMADMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incPDD", Randomizer.rand(levelData.IncPDDMin, levelData.IncPDDMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incMDD", Randomizer.rand(levelData.IncMDDMin, levelData.IncMDDMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incACC", Randomizer.rand(levelData.IncACCMin, levelData.IncACCMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incEVA", Randomizer.rand(levelData.IncEVAMin, levelData.IncEVAMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incSpeed", Randomizer.rand(levelData.IncSpeedMin, levelData.IncSpeedMax)));
            if (Randomizer.nextDouble() < 0.9)
                list.Add(new("incJump", Randomizer.rand(levelData.IncJumpMin, levelData.IncJumpMax)));
        }

        return list;
    }

    private bool canUseSkillBook(IPlayer player, int skillBookId)
    {
        var template = GetMasteryItemTemplate(skillBookId);
        if (template == null)
        {
            return false;
        }

        var targetSkillId = template.Skills.FirstOrDefault(x => x / 10000 == player.getJob().getId());
        if (targetSkillId == 0)
        {
            return false;
        }

        var skill2 = SkillFactory.getSkill(targetSkillId);
        return (player.getSkillLevel(skill2) >= template.ReqSkillLevel || template.ReqSkillLevel == 0)
            && player.getMasterLevel(skill2) < template.MasterLevel;
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
        var template = GetProvider(itemId).GetRequiredItem<IncubatorItemTemplate>(itemId);
        if (template == null || !template.HasUIData)
            return null;

        return new QuestConsItem
        {
            exp = template.Exp,
            grade = template.Grade,
            items = template.ConsumeItems.GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.Sum(u => u.Value)),
            questid = template.QuestID
        };
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

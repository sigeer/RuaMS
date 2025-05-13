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


using constants.game;
using constants.inventory;
using server;
using tools;

namespace client.inventory;


public class Equip : Item
{
    public enum ScrollResult
    {

        FAIL = 0, SUCCESS = 1, CURSE = 2
    }


    public enum StatUpgrade
    {

        incDEX = 0, incSTR = 1, incINT = 2, incLUK = 3,
        incMHP = 4, incMMP = 5, incPAD = 6, incMAD = 7,
        incPDD = 8, incMDD = 9, incEVA = 10, incACC = 11,
        incSpeed = 12, incJump = 13, incVicious = 14, incSlot = 15

    }


    private byte upgradeSlots;
    private byte level, itemLevel;
    private short flag;
    private int str, dex, _int, luk, hp, mp, watk, matk, wdef, mdef, acc, avoid, hands, speed, jump, vicious;
    private float itemExp;
    private int ringid = -1;
    private bool _wear = false;
    private bool isUpgradeable;    // timeless or reverse, or any equip that could levelup on GMS for all effects
    public bool IsElemental { get; }

    public Equip(int id, short position, int slots = 0) : base(id, position, 1)
    {
        log = LogFactory.GetLogger(LogType.Equip);
        this.upgradeSlots = (byte)slots;
        this.itemExp = 0;
        this.itemLevel = 1;

        IsElemental = (ItemInformationProvider.getInstance().getEquipLevel(id, false) > 1);
    }

    public override Item copy()
    {
        Equip ret = new Equip(getItemId(), getPosition(), getUpgradeSlots());
        ret.str = str;
        ret.dex = dex;
        ret._int = _int;
        ret.luk = luk;
        ret.hp = hp;
        ret.mp = mp;
        ret.matk = matk;
        ret.mdef = mdef;
        ret.watk = watk;
        ret.wdef = wdef;
        ret.acc = acc;
        ret.avoid = avoid;
        ret.hands = hands;
        ret.speed = speed;
        ret.jump = jump;
        ret.flag = flag;
        ret.vicious = vicious;
        ret.upgradeSlots = upgradeSlots;
        ret.itemLevel = itemLevel;
        ret.itemExp = itemExp;
        ret.level = level;
        ret.itemLog = new(itemLog);
        ret.setOwner(getOwner());
        ret.setQuantity(getQuantity());
        ret.setExpiration(getExpiration());
        ret.setGiftFrom(getGiftFrom());
        return ret;
    }

    public override short getFlag()
    {
        return flag;
    }

    public override sbyte getItemType()
    {
        return 1;
    }

    public byte getUpgradeSlots()
    {
        return upgradeSlots;
    }

    public int getStr()
    {
        return str;
    }

    public int getDex()
    {
        return dex;
    }

    public int getInt()
    {
        return _int;
    }

    public int getLuk()
    {
        return luk;
    }

    public int getHp()
    {
        return hp;
    }

    public int getMp()
    {
        return mp;
    }

    public int getWatk()
    {
        return watk;
    }

    public int getMatk()
    {
        return matk;
    }

    public int getWdef()
    {
        return wdef;
    }

    public int getMdef()
    {
        return mdef;
    }

    public int getAcc()
    {
        return acc;
    }

    public int getAvoid()
    {
        return avoid;
    }

    public int getHands()
    {
        return hands;
    }

    public int getSpeed()
    {
        return speed;
    }

    public int getJump()
    {
        return jump;
    }

    public int getVicious()
    {
        return vicious;
    }

    public override void setFlag(short flag)
    {
        this.flag = flag;
    }

    public void setStr(int str)
    {
        this.str = str;
    }

    public void setDex(int dex)
    {
        this.dex = dex;
    }

    public void setInt(int _int)
    {
        this._int = _int;
    }

    public void setLuk(int luk)
    {
        this.luk = luk;
    }

    public void setHp(int hp)
    {
        this.hp = hp;
    }

    public void setMp(int mp)
    {
        this.mp = mp;
    }

    public void setWatk(int watk)
    {
        this.watk = watk;
    }

    public void setMatk(int matk)
    {
        this.matk = matk;
    }

    public void setWdef(int wdef)
    {
        this.wdef = wdef;
    }

    public void setMdef(int mdef)
    {
        this.mdef = mdef;
    }

    public void setAcc(int acc)
    {
        this.acc = acc;
    }

    public void setAvoid(int avoid)
    {
        this.avoid = avoid;
    }

    public void setHands(int hands)
    {
        this.hands = hands;
    }

    public void setSpeed(int speed)
    {
        this.speed = speed;
    }

    public void setJump(int jump)
    {
        this.jump = jump;
    }

    public void setVicious(int vicious)
    {
        this.vicious = vicious;
    }

    public void setUpgradeSlots(byte upgradeSlots)
    {
        this.upgradeSlots = upgradeSlots;
    }

    public byte getLevel()
    {
        return level;
    }

    public void setLevel(byte level)
    {
        this.level = level;
    }

    private static int getStatModifier(bool isAttribute)
    {
        // each set of stat points grants a chance for a bonus stat point upgrade at equip level up.

        if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_POWER)
        {
            if (isAttribute)
            {
                return 2;
            }
            else
            {
                return 4;
            }
        }
        else
        {
            if (isAttribute)
            {
                return 4;
            }
            else
            {
                return 16;
            }
        }
    }

    private static int randomizeStatUpgrade(int top)
    {
        int limit = Math.Min(top, YamlConfig.config.server.MAX_EQUIPMNT_LVLUP_STAT_UP);

        int poolCount = (limit * (limit + 1) / 2) + limit;
        int rnd = Randomizer.rand(0, poolCount);

        int stat = 0;
        if (rnd >= limit)
        {
            rnd -= limit;
            stat = 1 + (int)Math.Floor((-1 + Math.Sqrt((8 * rnd) + 1)) / 2);    // optimized randomizeStatUpgrade author: David A.
        }

        return stat;
    }

    private static bool isPhysicalWeapon(int itemid)
    {
        Equip eqp = (Equip)ItemInformationProvider.getInstance().getEquipById(itemid);
        return eqp.getWatk() >= eqp.getMatk();
    }

    private bool isNotWeaponAffinity(StatUpgrade name)
    {
        // Vcoc's idea - WATK/MATK expected gains lessens outside of weapon affinity (physical/magic)

        if (ItemConstants.isWeapon(this.getItemId()))
        {
            if (name.Equals(StatUpgrade.incPAD))
            {
                return !isPhysicalWeapon(this.getItemId());
            }
            else if (name.Equals(StatUpgrade.incMAD))
            {
                return isPhysicalWeapon(this.getItemId());
            }
        }

        return false;
    }

    private void getUnitStatUpgrade(List<KeyValuePair<StatUpgrade, int>> stats, StatUpgrade name, int curStat, bool isAttribute)
    {
        isUpgradeable = true;

        int maxUpgrade = randomizeStatUpgrade((int)(1 + (curStat / (getStatModifier(isAttribute) * (isNotWeaponAffinity(name) ? 2.7 : 1)))));
        if (maxUpgrade == 0)
        {
            return;
        }

        stats.Add(new(name, maxUpgrade));
    }

    private static void getUnitSlotUpgrade(List<KeyValuePair<StatUpgrade, int>> stats, StatUpgrade name)
    {
        if (Randomizer.nextDouble() < 0.1)
        {
            stats.Add(new(name, 1));  // 10% success on getting a slot upgrade.
        }
    }

    private void improveDefaultStats(List<KeyValuePair<StatUpgrade, int>> stats)
    {
        if (dex > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incDEX, dex, true);
        }
        if (str > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incSTR, str, true);
        }
        if (_int > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incINT, _int, true);
        }
        if (luk > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incLUK, luk, true);
        }
        if (hp > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incMHP, hp, false);
        }
        if (mp > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incMMP, mp, false);
        }
        if (watk > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incPAD, watk, false);
        }
        if (matk > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incMAD, matk, false);
        }
        if (wdef > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incPDD, wdef, false);
        }
        if (mdef > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incMDD, mdef, false);
        }
        if (avoid > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incEVA, avoid, false);
        }
        if (acc > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incACC, acc, false);
        }
        if (speed > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incSpeed, speed, false);
        }
        if (jump > 0)
        {
            getUnitStatUpgrade(stats, StatUpgrade.incJump, jump, false);
        }
    }

    public Dictionary<StatUpgrade, int> getStats()
    {
        Dictionary<StatUpgrade, int> stats = new(5);

        if (dex > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incDEX, dex);
        }
        if (str > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incSTR, str);
        }
        if (_int > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incINT, _int);
        }
        if (luk > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incLUK, luk);
        }
        if (hp > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incMHP, hp);
        }
        if (mp > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incMMP, mp);
        }
        if (watk > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incPAD, watk);
        }
        if (matk > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incMAD, matk);
        }
        if (wdef > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incPDD, wdef);
        }
        if (mdef > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incMDD, mdef);
        }
        if (avoid > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incEVA, avoid);
        }
        if (acc > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incACC, acc);
        }
        if (speed > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incSpeed, speed);
        }
        if (jump > 0)
        {
            stats.AddOrUpdate(StatUpgrade.incJump, jump);
        }

        return stats;
    }

    public KeyValuePair<string, KeyValuePair<bool, bool>> gainStats(List<KeyValuePair<StatUpgrade, int>> stats)
    {
        bool gotSlot = false, gotVicious = false;
        string lvupStr = "";
        int statUp, maxStat = YamlConfig.config.server.MAX_EQUIPMNT_STAT;
        foreach (var stat in stats)
        {
            switch (stat.Key)
            {
                case StatUpgrade.incDEX:
                    statUp = Math.Min(stat.Value, maxStat - dex);
                    dex += statUp;
                    lvupStr += "+" + statUp + "DEX ";
                    break;
                case StatUpgrade.incSTR:
                    statUp = Math.Min(stat.Value, maxStat - str);
                    str += statUp;
                    lvupStr += "+" + statUp + "STR ";
                    break;
                case StatUpgrade.incINT:
                    statUp = Math.Min(stat.Value, maxStat - _int);
                    _int += statUp;
                    lvupStr += "+" + statUp + "INT ";
                    break;
                case StatUpgrade.incLUK:
                    statUp = Math.Min(stat.Value, maxStat - luk);
                    luk += statUp;
                    lvupStr += "+" + statUp + "LUK ";
                    break;
                case StatUpgrade.incMHP:
                    statUp = Math.Min(stat.Value, maxStat - hp);
                    hp += statUp;
                    lvupStr += "+" + statUp + "HP ";
                    break;
                case StatUpgrade.incMMP:
                    statUp = Math.Min(stat.Value, maxStat - mp);
                    mp += statUp;
                    lvupStr += "+" + statUp + "MP ";
                    break;
                case StatUpgrade.incPAD:
                    statUp = Math.Min(stat.Value, maxStat - watk);
                    watk += statUp;
                    lvupStr += "+" + statUp + "WATK ";
                    break;
                case StatUpgrade.incMAD:
                    statUp = Math.Min(stat.Value, maxStat - matk);
                    matk += statUp;
                    lvupStr += "+" + statUp + "MATK ";
                    break;
                case StatUpgrade.incPDD:
                    statUp = Math.Min(stat.Value, maxStat - wdef);
                    wdef += statUp;
                    lvupStr += "+" + statUp + "WDEF ";
                    break;
                case StatUpgrade.incMDD:
                    statUp = Math.Min(stat.Value, maxStat - mdef);
                    mdef += statUp;
                    lvupStr += "+" + statUp + "MDEF ";
                    break;
                case StatUpgrade.incEVA:
                    statUp = Math.Min(stat.Value, maxStat - avoid);
                    avoid += statUp;
                    lvupStr += "+" + statUp + "AVOID ";
                    break;
                case StatUpgrade.incACC:
                    statUp = Math.Min(stat.Value, maxStat - acc);
                    acc += statUp;
                    lvupStr += "+" + statUp + "ACC ";
                    break;
                case StatUpgrade.incSpeed:
                    statUp = Math.Min(stat.Value, maxStat - speed);
                    speed += statUp;
                    lvupStr += "+" + statUp + "SPEED ";
                    break;
                case StatUpgrade.incJump:
                    statUp = Math.Min(stat.Value, maxStat - jump);
                    jump += statUp;
                    lvupStr += "+" + statUp + "JUMP ";
                    break;

                case StatUpgrade.incVicious:
                    vicious -= stat.Value;
                    gotVicious = true;
                    break;
                case StatUpgrade.incSlot:
                    upgradeSlots += (byte)stat.Value;
                    gotSlot = true;
                    break;
            }
        }

        return new(lvupStr, new(gotSlot, gotVicious));
    }

    private void gainLevel(IClient c)
    {
        List<KeyValuePair<StatUpgrade, int>> stats = new();

        if (IsElemental)
        {
            var elementalStats = ItemInformationProvider.getInstance().getItemLevelupStats(getItemId(), itemLevel);

            foreach (KeyValuePair<string, int> p in elementalStats)
            {
                if (p.Value > 0)
                {
                    stats.Add(new(Enum.Parse<StatUpgrade>(p.Key), p.Value));
                }
            }
        }

        if (stats.Count > 0)
        {
            if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_SLOTS)
            {
                if (vicious > 0)
                {
                    getUnitSlotUpgrade(stats, StatUpgrade.incVicious);
                }
                getUnitSlotUpgrade(stats, StatUpgrade.incSlot);
            }
        }
        else
        {
            isUpgradeable = false;

            improveDefaultStats(stats);
            if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_SLOTS)
            {
                if (vicious > 0)
                {
                    getUnitSlotUpgrade(stats, StatUpgrade.incVicious);
                }
                getUnitSlotUpgrade(stats, StatUpgrade.incSlot);
            }

            if (isUpgradeable)
            {
                while (stats.Count == 0)
                {
                    improveDefaultStats(stats);
                    if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_SLOTS)
                    {
                        if (vicious > 0)
                        {
                            getUnitSlotUpgrade(stats, StatUpgrade.incVicious);
                        }
                        getUnitSlotUpgrade(stats, StatUpgrade.incSlot);
                    }
                }
            }
        }

        itemLevel++;

        string lvupStr = "'" + ItemInformationProvider.getInstance().getName(this.getItemId()) + "' is now level " + itemLevel + "! ";
        string showStr = "#e'" + ItemInformationProvider.getInstance().getName(this.getItemId()) + "'#b is now #elevel #r" + itemLevel + "#k#b!";

        var res = this.gainStats(stats);
        lvupStr += res.Key;
        bool gotSlot = res.Value.Key;
        bool gotVicious = res.Value.Value;

        if (gotVicious)
        {
            //c.getPlayer().dropMessage(6, "A new Vicious Hammer opportunity has been found on the '" + ItemInformationProvider.getInstance().getName(getItemId()) + "'!");
            lvupStr += "+VICIOUS ";
        }
        if (gotSlot)
        {
            //c.getPlayer().dropMessage(6, "A new upgrade slot has been found on the '" + ItemInformationProvider.getInstance().getName(getItemId()) + "'!");
            lvupStr += "+UPGSLOT ";
        }

        c.OnlinedCharacter.equipChanged();

        showLevelupMessage(showStr, c); // thanks to Polaris dev team !
        c.OnlinedCharacter.dropMessage(6, lvupStr);

        c.sendPacket(PacketCreator.showEquipmentLevelUp());
        c.OnlinedCharacter.getMap().broadcastPacket(c.OnlinedCharacter, PacketCreator.showForeignEffect(c.OnlinedCharacter.getId(), 15));
        c.OnlinedCharacter.forceUpdateItem(this);
    }

    public int getItemExp()
    {
        return (int)itemExp;
    }

    private static double normalizedMasteryExp(int reqLevel)
    {
        // Conversion factor between mob exp and equip exp gain. Through many calculations, the expected for equipment levelup
        // from level 1 to 2 is killing about 100~200 mobs of the same level range, on a 1x EXP rate scenario.

        if (reqLevel < 5)
        {
            return 42;
        }
        else if (reqLevel >= 78)
        {
            return Math.Max((10413.648 * Math.Exp(reqLevel * 0.03275)), 15);
        }
        else if (reqLevel >= 38)
        {
            return Math.Max((4985.818 * Math.Exp(reqLevel * 0.02007)), 15);
        }
        else if (reqLevel >= 18)
        {
            return Math.Max((248.219 * Math.Exp(reqLevel * 0.11093)), 15);
        }
        else
        {
            return Math.Max(((1334.564 * Math.Log(reqLevel)) - 1731.976), 15);
        }
    }

    object gainExpLock = new object();
    public void gainItemExp(IChannelClient c, int gain)
    {
        lock (gainExpLock)
        {
            // Ronan's Equip Exp gain method
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            if (!ii.isUpgradeable(this.getItemId()))
            {
                return;
            }

            int equipMaxLevel = Math.Min(30, Math.Max(ii.getEquipLevel(this.getItemId(), true), YamlConfig.config.server.USE_EQUIPMNT_LVLUP));
            if (itemLevel >= equipMaxLevel)
            {
                return;
            }

            int reqLevel = ii.getEquipLevelReq(this.getItemId());

            float masteryModifier = (float)(YamlConfig.config.server.EQUIP_EXP_RATE * ExpTable.getExpNeededForLevel(1)) / (float)normalizedMasteryExp(reqLevel);
            float elementModifier = (IsElemental) ? 0.85f : 0.6f;

            float baseExpGain = gain * elementModifier * masteryModifier;

            itemExp += baseExpGain;
            int expNeeded = ExpTable.getEquipExpNeededForLevel(itemLevel);

            if (YamlConfig.config.server.USE_DEBUG_SHOW_INFO_EQPEXP)
            {
                log.Debug("{ItemName} -> EXP Gain: {ItemGainExp}, Mastery: {Mastery}, Base gain: {ItemBaseGainExp}, exp: {ItemExp} / {ItemExpNeed}, Kills TNL: {0}", ii.getName(getItemId()),
                        gain, masteryModifier, baseExpGain, itemExp, expNeeded, expNeeded / (baseExpGain / c.OnlinedCharacter.getExpRate()));
            }

            if (itemExp >= expNeeded)
            {
                while (itemExp >= expNeeded)
                {
                    itemExp -= expNeeded;
                    gainLevel(c);

                    if (itemLevel >= equipMaxLevel)
                    {
                        itemExp = 0.0f;
                        break;
                    }

                    expNeeded = ExpTable.getEquipExpNeededForLevel(itemLevel);
                }
            }

            c.OnlinedCharacter.forceUpdateItem(this);
            //if(YamlConfig.config.server.USE_DEBUG) c.getPlayer().dropMessage("'" + ii.getName(this.getItemId()) + "': " + itemExp + " / " + expNeeded);
        }
    }

    public bool ReachedMaxLevel()
    {
        if (IsElemental)
        {
            if (itemLevel < ItemInformationProvider.getInstance().getEquipLevel(getItemId(), true))
            {
                return false;
            }
        }

        return itemLevel >= YamlConfig.config.server.USE_EQUIPMNT_LVLUP;
    }

    private static void showLevelupMessage(string msg, IClient c)
    {
        c.OnlinedCharacter.showHint(msg, 300);
    }

    public void setItemExp(int exp)
    {
        this.itemExp = exp;
    }

    public void setItemLevel(byte level)
    {
        this.itemLevel = level;
    }

    public override void setQuantity(short quantity)
    {
        if (quantity < 0 || quantity > 1)
        {
            throw new Exception("Setting the quantity to " + quantity + " on an equip (itemid: " + getItemId() + ")");
        }
        base.setQuantity(quantity);
    }

    public void setUpgradeSlots(int i)
    {
        this.upgradeSlots = (byte)i;
    }
    public int getRingId()
    {
        return ringid;
    }

    public void setRingId(int id)
    {
        this.ringid = id;
    }

    public bool isWearing()
    {
        return _wear;
    }

    public void wear(bool yes)
    {
        _wear = yes;
    }

    public byte getItemLevel()
    {
        return itemLevel;
    }

    public void SetDataFromDB(MtsItem dbModel)
    {
        this.setOwner(dbModel.Owner);
        this.setQuantity(1);
        this.setAcc((short)dbModel.Acc);
        this.setAvoid((short)dbModel.Avoid);
        this.setDex((short)dbModel.Dex);
        this.setHands((short)dbModel.Hands);
        this.setHp((short)dbModel.Hp);
        this.setInt((short)dbModel.Int);
        this.setJump((short)dbModel.Jump);
        this.setLuk((short)dbModel.Luk);
        this.setMatk((short)dbModel.Matk);
        this.setMdef((short)dbModel.Mdef);
        this.setMp((short)dbModel.Mp);
        this.setSpeed((short)dbModel.Speed);
        this.setStr((short)dbModel.Str);
        this.setWatk((short)dbModel.Watk);
        this.setWdef((short)dbModel.Wdef);
        this.setUpgradeSlots((byte)dbModel.Upgradeslots);
        this.setLevel((byte)dbModel.Level);
        this.setItemLevel((byte)dbModel.Itemlevel);
        this.setItemExp(dbModel.Itemexp);
        this.setRingId(dbModel.Ringid);
        this.setVicious((byte)dbModel.Vicious);
        this.setFlag((short)dbModel.Flag);
        this.setExpiration(dbModel.Expiration);
        this.setGiftFrom(dbModel.GiftFrom);

    }
}
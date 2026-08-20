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


using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Templates.Character;
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


    private sbyte upgradeSlots;
    private byte level, itemLevel;
    private int str, dex, _int, luk, hp, mp, watk, matk, wdef, mdef, acc, avoid, hands, speed, jump, vicious;
    private float itemExp;
    private bool _wear = false;

    public int MaxLevel => SourceTemplate.MaxLevel;
    /// <summary>
    /// 为什么不用bool，实际取skill时从template取：可支持装备在不同等级获得技能
    /// </summary>
    public Dictionary<int, int> Skills { get; set; } = new();
    public override EquipTemplate SourceTemplate { get; }

    public Equip(EquipTemplate template, short position, long uniqueId) : base(template.TemplateId, position, 1, uniqueId)
    {
        SourceTemplate = template;
        log = LogFactory.GetLogger(LogType.Equip);
        this.itemExp = 0;
        this.itemLevel = 1;
        this.quantity = 1;

    }

    public override Item copy()
    {
        Equip ret = new Equip(SourceTemplate, getPosition(), getUpgradeSlots());
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
        ret.vicious = vicious;
        ret.upgradeSlots = upgradeSlots;
        ret.itemLevel = itemLevel;
        ret.itemExp = itemExp;
        ret.level = level;
        ret.Skills = Skills.ToDictionary();

        CopyItemProps(ret);
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

    public sbyte getUpgradeSlots()
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

    public byte getLevel()
    {
        return level;
    }

    public void setLevel(byte level)
    {
        this.level = level;
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
                    statUp = stat.Value;
                    dex += statUp;
                    lvupStr += "+" + statUp + "DEX ";
                    break;
                case StatUpgrade.incSTR:
                    statUp = stat.Value;
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
                    upgradeSlots += (sbyte)stat.Value;
                    gotSlot = true;
                    break;
            }
        }

        return new(lvupStr, new(gotSlot, gotVicious));
    }

    public void ImproveSlot(int slot = 1)
    {
        List<KeyValuePair<StatUpgrade, int>> stats = [];
        if (vicious > 0)
        {
            stats.Add(new(StatUpgrade.incVicious, slot));
        }
        stats.Add(new(StatUpgrade.incSlot, slot));

        gainStats(stats);
    }

    private async Task gainLevel(IChannelClient c)
    {
        itemLevel++;

        var stats = ItemInformationProvider.getInstance().getItemLevelupStats(SourceTemplate, itemLevel - 1)
            .Where(x => x.Value > 0)
            .ToList();

        var skillData = SourceTemplate.GetActiveCase()?.SkillData?.FirstOrDefault(x => x.Level == itemLevel);
        if (skillData != null && skillData.Skills.Length > 0)
        {
            foreach (var skill in skillData.Skills)
            {
                var exsited = Skills.GetValueOrDefault(skill.SkillId);
                Skills[skill.SkillId] = exsited + skill.Level;
            }
        }

        string lvupStr = "'" + c.CurrentCulture.GetItemName(this.getItemId()) + "' is now level " + itemLevel + "! ";
        string showStr = "#e'" + c.CurrentCulture.GetItemName(this.getItemId()) + "'#b is now #elevel #r" + itemLevel + "#k#b!";

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

        await c.OnlinedCharacter.showHint(showStr, 300);
        await c.OnlinedCharacter.dropMessage(6, lvupStr);

        await c.SendPacket(PacketCreator.showEquipmentLevelUp());
        await c.OnlinedCharacter.BroadcastMap(PacketCreator.showForeignEffect(c.OnlinedCharacter.getId(), 15), c.OnlinedCharacter.Id);
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

    public async Task gainItemExp(IChannelClient c, int gain)
    {
        // Ronan's Equip Exp gain method
        if (!SourceTemplate.IsUpgradeable())
        {
            return;
        }

        if (itemLevel >= MaxLevel)
        {
            return;
        }

        int reqLevel = SourceTemplate.ReqLevel;

        float masteryModifier = (float)(YamlConfig.config.server.EQUIP_EXP_RATE * ExpTable.getExpNeededForLevel(1)) / (float)normalizedMasteryExp(reqLevel);
        float elementModifier = 0.6f;

        float baseExpGain = gain * elementModifier * masteryModifier;

        itemExp += baseExpGain;
        int expNeeded = ExpTable.getEquipExpNeededForLevel(itemLevel);

        if (YamlConfig.config.server.USE_DEBUG_SHOW_INFO_EQPEXP)
        {
            log.Debug("{ItemName} -> EXP Gain: {ItemGainExp}, Mastery: {Mastery}, Base gain: {ItemBaseGainExp}, exp: {ItemExp} / {ItemExpNeed}, Kills TNL: {0}",
                ClientCulture.SystemCulture.GetItemName(getItemId()),
                    gain, masteryModifier, baseExpGain, itemExp, expNeeded, expNeeded / (baseExpGain / c.OnlinedCharacter.getExpRate()));
        }

        if (itemExp >= expNeeded)
        {
            while (itemExp >= expNeeded)
            {
                itemExp -= expNeeded;
                await gainLevel(c);

                if (itemLevel >= MaxLevel)
                {
                    itemExp = 0.0f;
                    break;
                }

                expNeeded = ExpTable.getEquipExpNeededForLevel(itemLevel);
            }
        }

        await c.OnlinedCharacter.forceUpdateItem(this);
        //if(YamlConfig.config.server.USE_DEBUG) c.getPlayer().dropMessage("'" + ii.getName(this.getItemId()) + "': " + itemExp + " / " + expNeeded);
    }

    public bool ReachedMaxLevel()
    {
        return itemLevel >= MaxLevel;
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
        this.upgradeSlots = (sbyte)i;
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

    /// <summary>
    /// 需要重算效果（调用 equippedItem / unequippedItem）
    /// </summary>
    /// <param name="another"></param>
    /// <returns></returns>
    public bool NeedRecalcEffect(Equip another)
    {
        return getItemId() != another.getItemId();
    }

}
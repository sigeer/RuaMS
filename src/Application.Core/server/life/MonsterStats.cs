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

using Application.Shared.WzEntity;

namespace server.life;



/**
 * @author Frz
 */
public class MonsterStats
{
    public bool changeable;
    public int exp, hp, mp, level, PADamage, PDDamage, MADamage, MDDamage, dropPeriod, cp, buffToGive = -1, _removeAfter;
    public bool boss, undead, ffaLoot, _isExplosiveReward, firstAttack, _removeOnMiss;
    public string name;
    public Dictionary<string, int> animationTimes = new();
    public Dictionary<Element, ElementalEffectiveness> resistance = new();
    public int[] revives = new int[0];
    public byte tagColor, tagBgColor;
    public HashSet<MobSkillId> skills = new();
    public KeyValuePair<int, int>? cool = null;
    public BanishInfo? banish = null;
    public List<LoseItem>? _loseItem = null;
    public SelfDestruction? _selfDestruction = null;
    public int fixedStance = 0;
    public bool friendly;
    public void setChange(bool change)
    {
        this.changeable = change;
    }

    public bool isChangeable()
    {
        return changeable;
    }

    public int getExp()
    {
        return exp;
    }

    public void setExp(int exp)
    {
        this.exp = exp;
    }

    public int getHp()
    {
        return hp;
    }

    public void setHp(int hp)
    {
        this.hp = hp;
    }

    public int getMp()
    {
        return mp;
    }

    public void setMp(int mp)
    {
        this.mp = mp;
    }

    public int getLevel()
    {
        return level;
    }

    public void setLevel(int level)
    {
        this.level = level;
    }

    public int removeAfter()
    {
        return _removeAfter;
    }

    public void setRemoveAfter(int removeAfter)
    {
        this._removeAfter = removeAfter;
    }

    public int getDropPeriod()
    {
        return dropPeriod;
    }

    public void setDropPeriod(int dropPeriod)
    {
        this.dropPeriod = dropPeriod;
    }

    public void setBoss(bool boss)
    {
        this.boss = boss;
    }

    public bool isBoss()
    {
        return boss;
    }

    public void setFfaLoot(bool ffaLoot)
    {
        this.ffaLoot = ffaLoot;
    }

    public bool isFfaLoot()
    {
        return ffaLoot;
    }

    public void setAnimationTime(string name, int delay)
    {
        animationTimes.AddOrUpdate(name, delay);
    }

    public int getAnimationTime(string name)
    {
        return animationTimes.GetValueOrDefault(name, 500);
    }

    public bool isMobile()
    {
        return animationTimes.ContainsKey("move") || animationTimes.ContainsKey("fly");
    }

    public int[] getRevives()
    {
        return revives;
    }

    public void setRevives(int[] revives)
    {
        this.revives = revives;
    }

    public void setUndead(bool undead)
    {
        this.undead = undead;
    }

    public bool isUndead()
    {
        return undead;
    }

    public void setEffectiveness(Element e, ElementalEffectiveness ee)
    {
        resistance.AddOrUpdate(e, ee);
    }

    public ElementalEffectiveness getEffectiveness(Element e)
    {
        return resistance.GetValueOrDefault(e, ElementalEffectiveness.NORMAL);
    }

    public string getName()
    {
        return name;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public byte getTagColor()
    {
        return tagColor;
    }

    public void setTagColor(int tagColor)
    {
        this.tagColor = (byte)tagColor;
    }

    public byte getTagBgColor()
    {
        return tagBgColor;
    }

    public void setTagBgColor(int tagBgColor)
    {
        this.tagBgColor = (byte)tagBgColor;
    }

    public void setSkills(HashSet<MobSkillId> skills)
    {
        this.skills = skills;
    }

    public HashSet<MobSkillId> getSkills()
    {
        return this.skills.ToHashSet();
    }

    public int getNoSkills()
    {
        return this.skills.Count;
    }

    public bool hasSkill(int skillId, int level)
    {
        return skills.Any(x => x.type.getId() == skillId && x.level == level);
    }

    public void setFirstAttack(bool firstAttack)
    {
        this.firstAttack = firstAttack;
    }

    public bool isFirstAttack()
    {
        return firstAttack;
    }

    public void setBuffToGive(int buff)
    {
        this.buffToGive = buff;
    }

    public int getBuffToGive()
    {
        return buffToGive;
    }

    public void removeEffectiveness(Element e)
    {
        resistance.Remove(e);
    }

    public BanishInfo? getBanishInfo()
    {
        return banish;
    }

    public void setBanishInfo(BanishInfo? banish)
    {
        this.banish = banish;
    }

    public int getPADamage()
    {
        return PADamage;
    }

    public void setPADamage(int PADamage)
    {
        this.PADamage = PADamage;
    }

    public int getCP()
    {
        return cp;
    }

    public void setCP(int cp)
    {
        this.cp = cp;
    }

    public List<LoseItem>? loseItem()
    {
        return _loseItem;
    }

    public void addLoseItem(LoseItem li)
    {
        if (_loseItem == null)
        {
            _loseItem = new();
        }
        _loseItem.Add(li);
    }

    public SelfDestruction? selfDestruction()
    {
        return _selfDestruction;
    }

    public void setSelfDestruction(SelfDestruction? sd)
    {
        this._selfDestruction = sd;
    }

    public void setExplosiveReward(bool isExplosiveReward)
    {
        this._isExplosiveReward = isExplosiveReward;
    }

    public bool isExplosiveReward()
    {
        return _isExplosiveReward;
    }

    public void setRemoveOnMiss(bool removeOnMiss)
    {
        this._removeOnMiss = removeOnMiss;
    }

    public bool removeOnMiss()
    {
        return _removeOnMiss;
    }

    public void setCool(KeyValuePair<int, int> cool)
    {
        this.cool = cool;
    }

    public KeyValuePair<int, int>? getCool()
    {
        return cool;
    }

    public int getPDDamage()
    {
        return PDDamage;
    }

    public int getMADamage()
    {
        return MADamage;
    }

    public int getMDDamage()
    {
        return MDDamage;
    }

    public bool isFriendly()
    {
        return friendly;
    }

    public void setFriendly(bool value)
    {
        this.friendly = value;
    }

    public void setPDDamage(int PDDamage)
    {
        this.PDDamage = PDDamage;
    }

    public void setMADamage(int MADamage)
    {
        this.MADamage = MADamage;
    }

    public void setMDDamage(int MDDamage)
    {
        this.MDDamage = MDDamage;
    }

    public int getFixedStance()
    {
        return this.fixedStance;
    }

    public void setFixedStance(int stance)
    {
        this.fixedStance = stance;
    }

    public MonsterStats copy()
    {
        MonsterStats copy = new MonsterStats();
        try
        {
            setFields(this, copy);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            //try
            //{
            //    Thread.Sleep(10000);
            //}
            //catch (Exception ex)
            //{

            //}

        }

        return copy;
    }

    private static void setFields(object from, object to)
    {
        var fields = from.GetType().GetFields();
        foreach (var field in fields)
        {
            try
            {
                object? value = field.GetValue(from);
                field.SetValue(to, value);

            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
        }
    }
}

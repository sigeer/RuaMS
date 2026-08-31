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

using Application.Shared.Battle.Skills;
using Application.Shared.WzEntity;

namespace Application.Core.Server.life;



/// <summary>
/// @author Frz
/// 仅包含可供服务端覆盖的属性
/// </summary>
public class MonsterStats
{
    public bool IsChangeable { get; set; } = true;
    public int exp, level, dropPeriod, cp, buffToGive = -1, _removeAfter;
    public int MaxHP { get; set; }
    public int MaxMP { get; set; }
    public bool boss, ffaLoot, _isExplosiveReward, _removeOnMiss;

    public Dictionary<Element, ElementalEffectiveness> resistance = new();
    public int[] revives = new int[0];

    public KeyValuePair<int, int>? cool = null;
    public BanishInfo? banish = null;
    public List<LoseItem>? _loseItem = null;
    public SelfDestruction? _selfDestruction = null;
    public int fixedStance = 0;
    public bool friendly;
    public Dictionary<MobSkillId, int> MobSkillAnimation { get; set; } = new();

    public int getExp()
    {
        return exp;
    }

    public void setExp(int exp)
    {
        this.exp = exp;
    }

    public void SetMaxHP(int hp)
    {
        this.MaxHP = hp;
    }

    public void SetMaxMP(int mp)
    {
        this.MaxMP = mp;
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

    public int[] getRevives()
    {
        return revives;
    }

    public void setRevives(int[] revives)
    {
        this.revives = revives;
    }


    public void setEffectiveness(Element e, ElementalEffectiveness ee)
    {
        resistance.AddOrUpdate(e, ee);
    }

    public ElementalEffectiveness getEffectiveness(Element e)
    {
        return resistance.GetValueOrDefault(e, ElementalEffectiveness.NORMAL);
    }

    public HashSet<MobSkillId> getSkills()
    {
        return this.MobSkillAnimation.Keys.ToHashSet();
    }

    public int getNoSkills()
    {
        return this.MobSkillAnimation.Count;
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

    public void setCool(KeyValuePair<int, int> cool)
    {
        this.cool = cool;
    }

    public KeyValuePair<int, int>? getCool()
    {
        return cool;
    }


    public bool isFriendly()
    {
        return friendly;
    }

    public void setFriendly(bool value)
    {
        this.friendly = value;
    }
    public int getFixedStance()
    {
        return this.fixedStance;
    }

    public void setFixedStance(int stance)
    {
        this.fixedStance = stance;
    }
}

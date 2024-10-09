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


using Application.Core.Game.Skills;
using constants.skills;
using server;
using server.life;

namespace client;

public class Skill: ISkill
{
    private int id;
    private List<StatEffect> effects = new();
    private Element element;
    private int animationTime;
    private int job;
    private bool action;

    public Skill(int id)
    {
        this.id = id;
        this.job = id / 10000;
        element = Element.NEUTRAL;
    }

    public int getId()
    {
        return id;
    }

    public StatEffect getEffect(int level)
    {
        return effects.ElementAtOrDefault(level - 1) ?? throw new BusinessResException($"Skill Id = {id}, Level = {level}");
    }

    public int getMaxLevel()
    {
        return effects.Count;
    }

    public bool isFourthJob()
    {
        if (job == (int)Job.EVAN4)
        {
            return false;
        }
        if (id == Evan.MAGIC_MASTERY || id == Evan.FLAME_WHEEL || id == Evan.HEROS_WILL || id == Evan.DARK_FOG || id == Evan.SOUL_STONE)
        {
            return true;
        }
        return job % 10 == 2;
    }

    public void setElement(Element elem)
    {
        element = elem;
    }

    public Element getElement()
    {
        return element;
    }

    public int getAnimationTime()
    {
        return animationTime;
    }

    public void setAnimationTime(int time)
    {
        animationTime = time;
    }

    public void incAnimationTime(int time)
    {
        animationTime += time;
    }

    public bool isBeginnerSkill()
    {
        return id % 10000000 < 10000;
    }

    public void setAction(bool act)
    {
        action = act;
    }

    public bool getAction()
    {
        return action;
    }

    public void addLevelEffect(StatEffect effect)
    {
        effects.Add(effect);
    }
}
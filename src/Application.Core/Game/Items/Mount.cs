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
using Application.Core.Game.TheWorld;
using net.server;

namespace Application.Core.Game.Items;

/**
 * @author PurpleMadness < Patrick :O >
 */
public class Mount : IMount
{
    private int itemid;
    private int skillid;
    private int tiredness;
    private int exp;
    private int level;
    private IPlayer owner;
    private bool active;

    public IWorld WorldServer => Server.getInstance().getWorld(owner.World);

    public Mount(IPlayer owner, int id)
    {
        this.owner = owner;
        itemid = id;

        skillid = owner.getJobType() * 10000000 + 1004;
        tiredness = 0;
        level = 1;
        exp = 0;
        active = true;
    }

    public int getItemId()
    {
        return itemid;
    }

    public int getSkillId()
    {
        return skillid;
    }

    /**
     * 1902000 - Hog
     * 1902001 - Silver Mane
     * 1902002 - Red Draco
     * 1902005 - Mimiana
     * 1902006 - Mimio
     * 1902007 - Shinjou
     * 1902008 - Frog
     * 1902009 - Ostrich
     * 1902010 - Frog
     * 1902011 - Turtle
     * 1902012 - Yeti
     *
     * @return the id
     */
    public int getId()
    {
        if (itemid < 1903000)
        {
            return itemid - 1901999;
        }
        return 5;
    }

    public int getTiredness()
    {
        return tiredness;
    }

    public int getExp()
    {
        return exp;
    }

    public int getLevel()
    {
        return level;
    }

    public void setTiredness(int newtiredness)
    {
        tiredness = Math.Max(newtiredness, 0);
    }

    public int incrementAndGetTiredness()
    {
        tiredness++;
        return tiredness;
    }

    public void setExp(int newexp)
    {
        exp = newexp;
    }

    public void setLevel(int newlevel)
    {
        level = newlevel;
    }

    public void setItemId(int newitemid)
    {
        itemid = newitemid;
    }

    public void setSkillId(int newskillid)
    {
        skillid = newskillid;
    }

    public void setActive(bool set)
    {
        active = set;
    }

    public bool isActive()
    {
        return active;
    }

    public void empty()
    {
        if (owner != null && owner.IsOnlined)
        {
            WorldServer.unregisterMountHunger(owner);
        }
    }
}

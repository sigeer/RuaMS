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
namespace server.events.gm;

/**
 * @author kevintjuh93
 */
public class Coconuts
{
    private int id;
    private int hits = 0;
    private bool hittable = false;
    private long hittime = 0;
    Coconut _root;
    public Coconuts(Coconut coconut, int id)
    {
        _root = coconut;
        this.id = id;
        hittime = _root.Map.ChannelServer.Container.getCurrentTime();
    }

    public void hit()
    {
        this.hittime = _root.Map.ChannelServer.Container.getCurrentTime() + 750;
        hits++;
    }

    public int getHits()
    {
        return hits;
    }

    public void resetHits()
    {
        hits = 0;
    }

    public bool isHittable()
    {
        return hittable;
    }

    public void setHittable(bool hittable)
    {
        this.hittable = hittable;
    }

    public long getHitTime()
    {
        return hittime;
    }
}

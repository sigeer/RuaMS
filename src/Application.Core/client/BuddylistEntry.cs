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
namespace client;

public class BuddylistEntry
{
    private string name;
    private string group;
    private int cid;
    private int channel;
    private bool visible;

    /**
     * @param name
     * @param characterId
     * @param channel     should be -1 if the buddy is offline
     * @param visible
     */
    public BuddylistEntry(string name, string group, int characterId, int channel, bool visible)
    {
        this.name = name;
        this.group = group;
        this.cid = characterId;
        this.channel = channel;
        this.visible = visible;
    }

    /**
     * @return the channel the character is on. If the character is offline returns -1.
     */
    public int getChannel()
    {
        return channel;
    }

    public void setChannel(int channel)
    {
        this.channel = channel;
    }

    public bool isOnline()
    {
        return channel >= 0;
    }

    public string getName()
    {
        return name;
    }

    public string getGroup()
    {
        return group;
    }

    public int getCharacterId()
    {
        return cid;
    }

    public void setVisible(bool visible)
    {
        this.visible = visible;
    }

    public bool isVisible()
    {
        return visible;
    }

    public void changeGroup(string group)
    {
        this.group = group;
    }

    public override int GetHashCode()
    {
        int prime = 31;
        int result = 1;
        result = prime * result + cid;
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (obj == null)
        {
            return false;
        }

        if (obj is BuddylistEntry t)
            return t.cid == cid;
        return false;
    }
}

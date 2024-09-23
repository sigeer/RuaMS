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


/**
 * @author Danny
 */
public class Ring : IComparable<Ring>
{
    private int ringId;
    private int ringId2;
    private int partnerId;
    private int itemId;
    private string partnerName;
    private bool _equipped = false;

    public Ring(int id, int id2, int partnerId, int itemid, string partnername)
    {
        this.ringId = id;
        this.ringId2 = id2;
        this.partnerId = partnerId;
        this.itemId = itemid;
        this.partnerName = partnername;
    }

    public int getRingId()
    {
        return ringId;
    }

    public int getPartnerRingId()
    {
        return ringId2;
    }

    public int getPartnerChrId()
    {
        return partnerId;
    }

    public int getItemId()
    {
        return itemId;
    }

    public string getPartnerName()
    {
        return partnerName;
    }

    public bool equipped()
    {
        return _equipped;
    }

    public void equip()
    {
        this._equipped = true;
    }

    public void unequip()
    {
        this._equipped = false;
    }

    public override bool Equals(object? o)
    {
        return o is Ring ring && ring.getRingId() == getRingId();
    }

    public override int GetHashCode()
    {
        int hash = 5;
        hash = 53 * hash + this.ringId;
        return hash;
    }

    public int CompareTo(Ring? other)
    {
        if (other == null)
            return 1;

        if (ringId < other.getRingId())
        {
            return -1;
        }
        else if (ringId == other.getRingId())
        {
            return 0;
        }
        return 1;
    }
}

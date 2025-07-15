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

namespace Application.Core.Game.Relation;


/**
 * @author Danny
 */
public class Ring
{
    public int SourceId { get; set; }
    private long ringId;
    private long ringId2;
    private int partnerId;
    private int itemId;
    private string partnerName;
    private bool _equipped = false;
    public long PartnerRingId => ringId2;

    public Ring(int sourceId, long id, long id2, int partnerId, int itemid, string partnername)
    {
        SourceId = sourceId;
        ringId = id;
        ringId2 = id2;
        this.partnerId = partnerId;
        itemId = itemid;
        partnerName = partnername;
    }

    public long getRingId()
    {
        return ringId;
    }

    public long getPartnerRingId()
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
        _equipped = true;
    }

    public void unequip()
    {
        _equipped = false;
    }

    public override bool Equals(object? o)
    {
        return o is Ring ring && ring.SourceId == SourceId;
    }

    public override int GetHashCode()
    {
        return SourceId.GetHashCode();
    }

    public static bool operator ==(Ring? a, Ring? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Ring? a, Ring? b)
    {
        return !(a == b);
    }
}

public class RingSourceModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public long RingId1 { get; set; }
    public long RingId2 { get; set; }

    public int CharacterId1 { get; set; }
    public string CharacterName1 { get; set; } = null!;
    public int CharacterId2 { get; set; }
    public string CharacterName2 { get; set; } = null!;

    public bool Equiped { get; set; }

    public override bool Equals(object? o)
    {
        return o is RingSourceModel ring && ring.Id == Id;
    }

    public Ring? GetRing(long ringId)
    {
        if (ringId == RingId1)
            return new Ring(Id, RingId1, RingId2, CharacterId2, ItemId, CharacterName2);
        if (ringId == RingId2)
            return new Ring(Id, RingId2, RingId1, CharacterId1, ItemId, CharacterName1);

        return null;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(RingSourceModel? a, RingSourceModel? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(RingSourceModel? a, RingSourceModel? b)
    {
        return !(a == b);
    }
}
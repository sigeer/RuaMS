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


using Application.Core.model;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;

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

    public static Ring? loadFromDb(int ringId)
    {
        using var dbContext = new DBContext();
        return dbContext.Rings.Where(x => x.Id == ringId).ToList().Select(x => new Ring(x.Id, x.PartnerRingId, x.PartnerChrId, x.ItemId, x.PartnerName)).FirstOrDefault();
    }

    public static void removeRing(Ring? ring)
    {
        try
        {
            if (ring == null)
            {
                return;
            }

            using var dbContext = new DBContext();
            dbContext.Rings.Where(x => x.Id == ring.getRingId() || x.Id == ring.getPartnerRingId()).ExecuteDelete();

            CashIdGenerator.freeCashId(ring.getRingId());
            CashIdGenerator.freeCashId(ring.getPartnerRingId());

            dbContext.Inventoryequipments.Where(x => x.RingId == ring.getRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
            dbContext.Inventoryequipments.Where(x => x.RingId == ring.getPartnerRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
        }
    }

    public static RingPair createRing(int itemid, IPlayer partner1, IPlayer partner2)
    {
        try
        {
            if (partner1 == null)
            {
                return new(-3, -3);
            }
            else if (partner2 == null)
            {
                return new(-2, -2);
            }

            int[] ringID = new int[2];
            ringID[0] = CashIdGenerator.generateCashId();
            ringID[1] = CashIdGenerator.generateCashId();

            using var dbContext = new DBContext();
            var dbModel = new DB_Ring(ringID[0], itemid, ringID[1], partner2.getId(), partner2.getName());

            dbContext.Rings.Add(dbModel);

            var dbRelatedModel = new DB_Ring(ringID[1], itemid, ringID[0], partner1.getId(), partner1.getName());
            dbContext.Rings.Add(dbRelatedModel);
            return new(ringID[0], ringID[1]);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
            return new(-1, -1);
        }
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
        if (o is Ring ring)
        {
            return ring.getRingId() == getRingId();
        }
        return false;
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

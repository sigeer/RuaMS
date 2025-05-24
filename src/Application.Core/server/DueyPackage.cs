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


using client.inventory;

namespace server;

public class DueyPackage
{
    private string sender = null;
    private Item? item = null;
    private int mesos = 0;
    private string? message = null;
    private DateTimeOffset timestamp;
    private int packageId = 0;

    public DueyPackage(int pId, Item item)
    {
        this.item = item;
        packageId = pId;
    }

    public DueyPackage(int pId)
    { // Meso only package.
        this.packageId = pId;
    }

    public string getSender()
    {
        return sender;
    }

    public void setSender(string name)
    {
        sender = name;
    }

    public Item getItem()
    {
        return item;
    }

    public int getMesos()
    {
        return mesos;
    }

    public void setMesos(int set)
    {
        mesos = set;
    }

    public string? getMessage()
    {
        return message;
    }

    public void setMessage(string? m)
    {
        message = m;
    }

    public int getPackageId()
    {
        return packageId;
    }

    public long sentTimeInMilliseconds()
    {
        return timestamp.AddMonths(1).ToUnixTimeMilliseconds();
    }

    public bool isDeliveringTime()
    {
        return timestamp >= DateTimeOffset.UtcNow;
    }

    public void setSentTime(DateTimeOffset ts, bool quick)
    {
        DateTimeOffset cal = ts;

        if (quick)
        {
            if (DateTimeOffset.UtcNow - ts < TimeSpan.FromDays(1))
            {  // thanks inhyuk for noticing quick delivery packages unavailable to retrieve from the get-go
                cal.AddDays(-1);
            }
        }

        this.timestamp = cal;
    }
}

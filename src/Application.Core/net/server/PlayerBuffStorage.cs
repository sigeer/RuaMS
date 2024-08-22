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


using client;
using tools;

namespace net.server;


/**
 * @author Danny//changed to map :3
 * @author Ronan//debuffs to storage as well
 */
public class PlayerBuffStorage
{
    private int id = (int)(Randomizer.nextDouble() * 100);
    private object lockObj = new object();
    private Dictionary<int, List<PlayerBuffValueHolder>> buffs = new();
    private Dictionary<int, Dictionary<Disease, DiseaseExpiration>> diseases = new();

    public void addBuffsToStorage(int chrid, List<PlayerBuffValueHolder> toStore)
    {
        Monitor.Enter(lockObj);
        try
        {
            buffs.AddOrUpdate(chrid, toStore);//Old one will be replaced if it's in here.
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public List<PlayerBuffValueHolder>? getBuffsFromStorage(int chrid)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (buffs.Remove(chrid, out var d))
                return d;

            return null;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void addDiseasesToStorage(int chrid, Dictionary<Disease, DiseaseExpiration> toStore)
    {
        Monitor.Enter(lockObj);
        try
        {
            diseases.AddOrUpdate(chrid, toStore);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Dictionary<Disease, DiseaseExpiration>? getDiseasesFromStorage(int chrid)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (diseases.Remove(chrid, out var d))
                return d;
            return null;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public override int GetHashCode()
    {
        int prime = 31;
        int result = 1;
        result = prime * result + id;
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

        if (obj is PlayerBuffStorage o)
            return id == o.id;
        return false;
    }
}

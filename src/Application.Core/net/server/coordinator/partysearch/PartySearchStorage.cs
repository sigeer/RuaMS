/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using tools;

namespace net.server.coordinator.partysearch;


/**
 * @author Ronan
 */
public class PartySearchStorage
{

    private List<PartySearchCharacter> storage = new(20);
    private IntervalBuilder emptyIntervals = new IntervalBuilder();

    ReaderWriterLockSlim psLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public PartySearchStorage()
    {
    }

    public List<PartySearchCharacter> getStorageList()
    {
        psLock.EnterReadLock();
        try
        {
            return new(storage);
        }
        finally
        {
            psLock.ExitReadLock();
        }
    }

    private Dictionary<int, IPlayer> fetchRemainingPlayers()
    {
        List<PartySearchCharacter> players = getStorageList();
        Dictionary<int, IPlayer> remainingPlayers = new(players.Count);

        foreach (var psc in players)
        {
            if (psc.isQueued())
            {
                var chr = psc.getPlayer();
                if (chr != null)
                {
                    remainingPlayers.AddOrUpdate(chr.getId(), chr);
                }
            }
        }

        return remainingPlayers;
    }

    public void updateStorage(ICollection<IPlayer> echelon)
    {
        Dictionary<int, IPlayer> newcomers = new();
        foreach (IPlayer chr in echelon)
        {
            newcomers.AddOrUpdate(chr.getId(), chr);
        }

        Dictionary<int, IPlayer> curStorage = fetchRemainingPlayers();
        curStorage.putAll(newcomers);

        List<PartySearchCharacter> pscList = curStorage.Values.Select(x => new PartySearchCharacter(x)).OrderBy(x => x.getLevel()).ToList();

        psLock.EnterWriteLock();
        try
        {
            storage.Clear();
            storage.AddRange(pscList);
        }
        finally
        {
            psLock.ExitWriteLock();
        }

        emptyIntervals.clear();
    }

    private static int bsearchStorage(List<PartySearchCharacter> storage, int level)
    {
        int st = 0, en = storage.Count - 1;

        int mid, idx;
        while (en >= st)
        {
            idx = (st + en) / 2;
            mid = storage.get(idx).getLevel();

            if (mid == level)
            {
                return idx;
            }
            else if (mid < level)
            {
                st = idx + 1;
            }
            else
            {
                en = idx - 1;
            }
        }

        return en;
    }

    public IPlayer? callPlayer(int callerCid, int callerMapid, int minLevel, int maxLevel)
    {
        if (emptyIntervals.inInterval(minLevel, maxLevel))
        {
            return null;
        }

        List<PartySearchCharacter> pscList = getStorageList();

        int idx = bsearchStorage(pscList, maxLevel);
        for (int i = idx; i >= 0; i--)
        {
            PartySearchCharacter psc = pscList.get(i);
            if (!psc.isQueued())
            {
                continue;
            }

            if (psc.getLevel() < minLevel)
            {
                break;
            }

            var chr = psc.callPlayer(callerCid, callerMapid);
            if (chr != null)
            {
                return chr;
            }
        }

        emptyIntervals.addInterval(minLevel, maxLevel);
        return null;
    }

    public void detachPlayer(IPlayer chr)
    {
        PartySearchCharacter? toRemove = null;
        foreach (PartySearchCharacter psc in getStorageList())
        {
            var player = psc.getPlayer();

            if (player != null && player.getId() == chr.getId())
            {
                toRemove = psc;
                break;
            }
        }

        if (toRemove != null)
        {
            psLock.EnterWriteLock();
            try
            {
                storage.Remove(toRemove);
            }
            finally
            {
                psLock.ExitWriteLock();
            }
        }
    }

}

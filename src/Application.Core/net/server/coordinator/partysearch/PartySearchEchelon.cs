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


using client;

namespace net.server.coordinator.partysearch;

/**
 * @author Ronan
 */
public class PartySearchEchelon
{
    ReaderWriterLockSlim psLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Dictionary<int, WeakReference<Character>> echelon = new(20);

    public PartySearchEchelon()
    {
    }

    public List<Character> exportEchelon()
    {
        psLock.EnterWriteLock();     // reversing read/write actually could provide a lax yet sure performance/precision trade-off here
        try
        {
            List<Character> players = new(echelon.Count);

            foreach (WeakReference<Character> chrRef in echelon.Values)
            {
                if (chrRef.TryGetTarget(out var chr) && chr != null)
                    players.Add(chr);
            }

            echelon.Clear();
            return players;
        }
        finally
        {
            psLock.ExitWriteLock();
        }
    }

    public void attachPlayer(Character chr)
    {
        psLock.EnterReadLock();
        try
        {
            echelon.AddOrUpdate(chr.getId(), new(chr));
        }
        finally
        {
            psLock.ExitReadLock();
        }
    }

    public bool detachPlayer(Character chr)
    {
        psLock.EnterReadLock();
        try
        {
            return echelon.Remove(chr.getId(), out var d) && d != null;
        }
        finally
        {
            psLock.ExitReadLock();
        }
    }

}

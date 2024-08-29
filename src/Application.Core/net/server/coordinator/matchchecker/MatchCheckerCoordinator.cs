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


namespace net.server.coordinator.matchchecker;



/**
 * @author Ronan
 */
public class MatchCheckerCoordinator
{

    private Dictionary<int, MatchCheckingElement> matchEntries = new();

    private HashSet<int> pooledCids = new();
    private Semaphore semaphorePool = new Semaphore(7, 7);

    public class MatchCheckingEntry
    {
        public bool accepted;
        public int cid;

        public MatchCheckingEntry(int cid)
        {
            this.cid = cid;
            this.accepted = false;
        }

        public bool setAccept()
        {
            if (!this.accepted)
            {
                this.accepted = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool getAccept()
        {
            return this.accepted;
        }
    }

    public class MatchCheckingElement
    {
        public int leaderCid;
        public int world;

        public MatchCheckerType matchType;
        public AbstractMatchCheckerListener listener;

        public Dictionary<int, MatchCheckingEntry> confirmingMembers = new();
        public int confirmCount;
        public bool active = true;

        private string message;

        public MatchCheckingElement(MatchCheckerType matchType, int leaderCid, int world, AbstractMatchCheckerListener leaderListener, HashSet<int> matchPlayers, string message)
        {
            this.leaderCid = leaderCid;
            this.world = world;
            this.listener = leaderListener;
            this.confirmCount = 0;
            this.message = message;
            this.matchType = matchType;

            foreach (int cid in matchPlayers)
            {
                MatchCheckingEntry mmcEntry = new MatchCheckingEntry(cid);
                confirmingMembers.AddOrUpdate(cid, mmcEntry);
            }
        }

        public bool acceptEntry(int cid)
        {
            var mmcEntry = confirmingMembers.GetValueOrDefault(cid);
            if (mmcEntry != null)
            {
                if (mmcEntry.setAccept())
                {
                    this.confirmCount++;

                    return this.confirmCount == this.confirmingMembers.Count;
                }
            }

            return false;
        }

        public bool isMatchActive()
        {
            return active;
        }

        public void setMatchActive(bool a)
        {
            active = a;
        }

        public HashSet<int> getMatchPlayers()
        {
            return confirmingMembers.Keys.ToHashSet();
        }

        public HashSet<int> getAcceptedMatchPlayers()
        {
            HashSet<int> s = new();

            foreach (var e in confirmingMembers)
            {
                if (e.Value.getAccept())
                {
                    s.Add(e.Key);
                }
            }

            return s;
        }

        public HashSet<IPlayer> getMatchCharacters()
        {
            HashSet<IPlayer> players = new();

            var wserv = Server.getInstance().getWorld(world);
            if (wserv != null)
            {
                var ps = wserv.getPlayerStorage();

                foreach (int cid in getMatchPlayers())
                {
                    var chr = ps.getCharacterById(cid);
                    if (chr != null && chr.IsOnlined)
                    {
                        players.Add(chr);
                    }
                }
            }

            return players;
        }

        public void dispatchMatchCreated()
        {
            HashSet<IPlayer> nonLeaderMatchPlayers = getMatchCharacters();
            var leader = nonLeaderMatchPlayers.FirstOrDefault(x => x.getId() == leaderCid);
            if (leader != null)
            {
                nonLeaderMatchPlayers.Remove(leader);
                listener.onMatchCreated(leader, nonLeaderMatchPlayers, message);
            }
        }

        public void dispatchMatchResult(bool accept)
        {
            if (accept)
            {
                listener.onMatchAccepted(leaderCid, getMatchCharacters(), message);
            }
            else
            {
                listener.onMatchDeclined(leaderCid, getMatchCharacters(), message);
            }
        }

        public void dispatchMatchDismissed()
        {
            listener.onMatchDismissed(leaderCid, getMatchCharacters(), message);
        }
    }

    private void unpoolMatchPlayer(int cid)
    {
        unpoolMatchPlayers([cid]);
    }

    private void unpoolMatchPlayers(HashSet<int> matchPlayers)
    {
        foreach (int cid in matchPlayers)
        {
            pooledCids.Remove(cid);
        }
    }

    private bool poolMatchPlayer(int cid)
    {
        return poolMatchPlayers([cid]);
    }

    private bool poolMatchPlayers(HashSet<int> matchPlayers)
    {
        HashSet<int> pooledPlayers = new();

        foreach (int cid in matchPlayers)
        {
            if (!pooledCids.Add(cid))
            {
                unpoolMatchPlayers(pooledPlayers);
                return false;
            }
            else
            {
                pooledPlayers.Add(cid);
            }
        }

        return true;
    }

    private bool isMatchingAvailable(HashSet<int> matchPlayers)
    {
        foreach (int cid in matchPlayers)
        {
            if (matchEntries.ContainsKey(cid))
            {
                return false;
            }
        }

        return true;
    }

    private void reenablePlayerMatching(HashSet<int> matchPlayers)
    {
        foreach (int cid in matchPlayers)
        {
            var mmce = matchEntries.GetValueOrDefault(cid);

            if (mmce != null)
            {
                lock (mmce)
                {
                    if (!mmce.isMatchActive())
                    {
                        matchEntries.Remove(cid);
                    }
                }
            }
        }
    }

    public int getMatchConfirmationLeaderid(int cid)
    {
        var mmce = matchEntries.GetValueOrDefault(cid);
        if (mmce != null)
        {
            return mmce.leaderCid;
        }
        else
        {
            return -1;
        }
    }

    public MatchCheckerType? getMatchConfirmationType(int cid)
    {
        var mmce = matchEntries.GetValueOrDefault(cid);
        if (mmce != null)
        {
            return mmce.matchType;
        }
        else
        {
            return null;
        }
    }

    public bool isMatchConfirmationActive(int cid)
    {
        var mmce = matchEntries.GetValueOrDefault(cid);
        if (mmce != null)
        {
            return mmce.active;
        }
        else
        {
            return false;
        }
    }

    private MatchCheckingElement createMatchConfirmationInternal(MatchCheckerType matchType, int world, int leaderCid, AbstractMatchCheckerListener leaderListener, HashSet<int> players, string message)
    {
        MatchCheckingElement mmce = new MatchCheckingElement(matchType, leaderCid, world, leaderListener, players, message);

        foreach (int cid in players)
        {
            matchEntries.AddOrUpdate(cid, mmce);
        }

        acceptMatchElement(mmce, leaderCid);
        return mmce;
    }

    public bool createMatchConfirmation(MatchCheckerType matchType, int world, int leaderCid, HashSet<int> players, string message)
    {
        MatchCheckingElement? mmce = null;
        try
        {
            semaphorePool.WaitOne();
            try
            {
                if (poolMatchPlayers(players))
                {
                    try
                    {
                        if (isMatchingAvailable(players))
                        {
                            AbstractMatchCheckerListener leaderListener = matchType.getListener();
                            mmce = createMatchConfirmationInternal(matchType, world, leaderCid, leaderListener, players, message);
                        }
                        else
                        {
                            reenablePlayerMatching(players);
                        }
                    }
                    finally
                    {
                        unpoolMatchPlayers(players);
                    }
                }
            }
            finally
            {
                semaphorePool.Release();
            }
        }
        catch (ThreadInterruptedException ie)
        {
            Log.Logger.Error(ie.ToString());
        }

        if (mmce != null)
        {
            mmce.dispatchMatchCreated();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void disposeMatchElement(MatchCheckingElement mmce)
    {
        HashSet<int> matchPlayers = mmce.getMatchPlayers();     // thanks Ai for noticing players getting match-stuck on certain cases
        while (!poolMatchPlayers(matchPlayers))
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        try
        {
            foreach (int cid in matchPlayers)
            {
                matchEntries.Remove(cid);
            }
        }
        finally
        {
            unpoolMatchPlayers(matchPlayers);
        }
    }

    private bool acceptMatchElement(MatchCheckingElement mmce, int cid)
    {
        if (mmce.acceptEntry(cid))
        {
            unpoolMatchPlayer(cid);
            disposeMatchElement(mmce);

            return true;
        }
        else
        {
            return false;
        }
    }

    private void denyMatchElement(MatchCheckingElement mmce, int cid)
    {
        unpoolMatchPlayer(cid);
        disposeMatchElement(mmce);
    }

    private void dismissMatchElement(MatchCheckingElement mmce, int cid)
    {
        mmce.setMatchActive(false);

        unpoolMatchPlayer(cid);
        disposeMatchElement(mmce);
    }

    public bool answerMatchConfirmation(int cid, bool accept)
    {
        MatchCheckingElement? mmce = null;
        try
        {
            semaphorePool.WaitOne();
            try
            {
                while (matchEntries.ContainsKey(cid))
                {
                    if (poolMatchPlayer(cid))
                    {
                        try
                        {
                            mmce = matchEntries.GetValueOrDefault(cid);

                            if (mmce != null)
                            {
                                Monitor.Enter(mmce);
                                try
                                {
                                    if (!mmce.isMatchActive())
                                    {    // thanks Alex (Alex-0000) for noticing that exploiters could stall on match checking
                                        matchEntries.Remove(cid);
                                        Monitor.Exit(mmce);
                                        mmce = null;
                                    }
                                    else
                                    {
                                        if (accept)
                                        {
                                            if (!acceptMatchElement(mmce, cid))
                                            {
                                                Monitor.Exit(mmce);
                                                mmce = null;
                                            }

                                            break;  // thanks Rohenn for noticing loop scenario here
                                        }
                                        else
                                        {
                                            denyMatchElement(mmce, cid);
                                            matchEntries.Remove(cid);
                                        }
                                    }
                                }
                                finally
                                {
                                    if (mmce != null)
                                        Monitor.Exit(mmce);
                                }
                            }
                        }
                        finally
                        {
                            unpoolMatchPlayer(cid);
                        }
                    }
                }
            }
            finally
            {
                semaphorePool.Release();
            }
        }
        catch (ThreadInterruptedException ie)
        {
            Log.Logger.Error(ie.ToString());
        }

        if (mmce != null)
        {
            mmce.dispatchMatchResult(accept);
        }

        return false;
    }

    public bool dismissMatchConfirmation(int cid)
    {
        MatchCheckingElement? mmce = null;
        try
        {
            semaphorePool.WaitOne();
            try
            {
                while (matchEntries.ContainsKey(cid))
                {
                    if (poolMatchPlayer(cid))
                    {
                        try
                        {
                            mmce = matchEntries.GetValueOrDefault(cid);

                            if (mmce != null)
                            {
                                Monitor.Enter(mmce);
                                try
                                {
                                    if (!mmce.isMatchActive())
                                    {
                                        Monitor.Exit(mmce);
                                        mmce = null;
                                    }
                                    else
                                    {
                                        dismissMatchElement(mmce, cid);
                                    }
                                }
                                finally
                                {
                                    if (mmce != null)
                                        Monitor.Exit(mmce);
                                }
                            }
                        }
                        finally
                        {
                            unpoolMatchPlayer(cid);
                        }
                    }
                }
            }
            finally
            {
                semaphorePool.Release();
            }
        }
        catch (ThreadInterruptedException ie)
        {
            Log.Logger.Error(ie.ToString());
        }

        if (mmce != null)
        {
            mmce.dispatchMatchDismissed();
            return true;
        }
        else
        {
            return false;
        }
    }

}

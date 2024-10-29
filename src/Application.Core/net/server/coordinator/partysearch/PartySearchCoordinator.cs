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
using constants.id;
using net.server.coordinator.world;
using tools;

namespace net.server.coordinator.partysearch;

/**
 * @author Ronan
 */
public class PartySearchCoordinator
{

    private Dictionary<Job, PartySearchStorage> storage = new();
    private Dictionary<Job, PartySearchEchelon> upcomers = new();

    private List<IPlayer> leaderQueue = new();

    private Dictionary<int, IPlayer> searchLeaders = new();
    private Dictionary<int, LeaderSearchMetadata> searchSettings = new();

    private Dictionary<IPlayer, LeaderSearchMetadata> timeoutLeaders = new();

    private int updateCount = 0;

    private static Dictionary<int, HashSet<int>> mapNeighbors = fetchNeighbouringMaps();
    private static Dictionary<int, Job> jobTable = instantiateJobTable();

    ReaderWriterLockSlim leaderQueueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    public PartySearchCoordinator()
    {
        foreach (Job job in Enum.GetValues<Job>())
        {
            storage.AddOrUpdate(job, new PartySearchStorage());
            upcomers.AddOrUpdate(job, new PartySearchEchelon());
        }
    }

    private static Dictionary<int, HashSet<int>> fetchNeighbouringMaps()
    {
        Dictionary<int, HashSet<int>> mapLinks = new();

        Data data = DataProviderFactory.getDataProvider(WZFiles.ETC).getData("MapNeighbors.img");
        if (data != null)
        {
            foreach (Data mapdata in data.getChildren())
            {
                int mapid = int.Parse(mapdata.getName());

                HashSet<int> neighborMaps = new();
                mapLinks.AddOrUpdate(mapid, neighborMaps);

                foreach (Data neighbordata in mapdata.getChildren())
                {
                    int neighborid = DataTool.getInt(neighbordata, MapId.NONE);

                    if (neighborid != MapId.NONE)
                    {
                        neighborMaps.Add(neighborid);
                    }
                }
            }
        }

        return mapLinks;
    }

    public static bool isInVicinity(int callerMapid, int calleeMapid)
    {
        var vicinityMapids = mapNeighbors.GetValueOrDefault(calleeMapid);

        if (vicinityMapids != null)
        {
            return vicinityMapids.Contains(calleeMapid);
        }
        else
        {
            int callerRange = callerMapid / 10000000;
            if (callerRange >= 90)
            {
                return callerRange == (calleeMapid / 1000000);
            }
            else
            {
                return callerRange == (calleeMapid / 10000000);
            }
        }
    }

    private static Dictionary<int, Job> instantiateJobTable()
    {
        Dictionary<int, Job> table = new();

        List<KeyValuePair<int, int>> jobSearchTypes =
        [
            new(Job.MAPLELEAF_BRIGADIER.getId(), 0),
            new(0, 0),
            new(Job.ARAN1.getId(), 0),
            new(100, 3),
            new(Job.DAWNWARRIOR1.getId(), 0),
            new(200, 3),
            new(Job.BLAZEWIZARD1.getId(), 0),
            new(500, 2),
            new(Job.THUNDERBREAKER1.getId(), 0),
            new(400, 2),
            new(Job.NIGHTWALKER1.getId(), 0),
            new(300, 2),
            new(Job.WINDARCHER1.getId(), 0),
            new(Job.EVAN1.getId(), 0),
        ];


        int i = 0;
        foreach (var p in jobSearchTypes)
        {
            table.AddOrUpdate(i, JobUtils.getById(p.Key));
            i++;

            for (int j = 1; j <= p.Value; j++)
            {
                table.AddOrUpdate(i, JobUtils.getById(p.Key + 10 * j));
                i++;
            }
        }

        return table;
    }

    private class LeaderSearchMetadata
    {
        public int minLevel;
        public int maxLevel;
        public List<Job> searchedJobs;

        public int reentryCount;

        private List<Job> decodeSearchedJobs(int jobsSelected)
        {
            List<Job> searchedJobs = new();

            int topByte = (int)((Math.Log(jobsSelected) / Math.Log(2)) + 1e-5);

            for (int i = 0; i <= topByte; i++)
            {
                if (jobsSelected % 2 == 1)
                {
                    var job = jobTable.get(i);
                    if (job != null)
                    {
                        searchedJobs.Add(job.Value);
                    }
                }

                jobsSelected = jobsSelected >> 1;
                if (jobsSelected == 0)
                {
                    break;
                }
            }

            return searchedJobs;
        }

        public LeaderSearchMetadata(int minLevel, int maxLevel, int jobs)
        {
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.searchedJobs = decodeSearchedJobs(jobs);
            this.reentryCount = 0;
        }

    }

    public void attachPlayer(IPlayer chr)
    {
        upcomers.GetValueOrDefault(getPartySearchJob(chr.getJob()))!.attachPlayer(chr);
    }

    public void detachPlayer(IPlayer chr)
    {
        Job psJob = getPartySearchJob(chr.getJob());

        if (!upcomers.GetValueOrDefault(psJob)!.detachPlayer(chr))
        {
            storage.GetValueOrDefault(psJob)!.detachPlayer(chr);
        }
    }

    public void updatePartySearchStorage()
    {
        foreach (var psUpdate in upcomers)
        {
            storage.GetValueOrDefault(psUpdate.Key)!.updateStorage(psUpdate.Value.exportEchelon());
        }
    }

    private static Job getPartySearchJob(Job job)
    {
        if (job.getJobNiche() == 0)
        {
            return Job.BEGINNER;
        }
        else if (job.getId() < 600)
        { // explorers
            return JobUtils.getById((job.getId() / 10) * 10);
        }
        else if (job.getId() >= 1000)
        {
            return JobUtils.getById((job.getId() / 100) * 100);
        }
        else
        {
            return Job.MAPLELEAF_BRIGADIER;
        }
    }

    private IPlayer? fetchPlayer(int callerCid, int callerMapid, Job job, int minLevel, int maxLevel)
    {
        return storage.GetValueOrDefault(getPartySearchJob(job))!.callPlayer(callerCid, callerMapid, minLevel, maxLevel);
    }

    private void addQueueLeader(IPlayer leader)
    {
        leaderQueueLock.EnterReadLock();
        try
        {
            leaderQueue.Add(leader);
        }
        finally
        {
            leaderQueueLock.ExitReadLock();
        }
    }

    private void removeQueueLeader(IPlayer leader)
    {
        leaderQueueLock.EnterReadLock();
        try
        {
            leaderQueue.Remove(leader);
        }
        finally
        {
            leaderQueueLock.ExitReadLock();
        }
    }

    public void registerPartyLeader(IPlayer leader, int minLevel, int maxLevel, int jobs)
    {
        if (searchLeaders.ContainsKey(leader.getId()))
        {
            return;
        }

        searchSettings.AddOrUpdate(leader.getId(), new LeaderSearchMetadata(minLevel, maxLevel, jobs));
        searchLeaders.AddOrUpdate(leader.getId(), leader);
        addQueueLeader(leader);
    }

    private void registerPartyLeader(IPlayer leader, LeaderSearchMetadata settings)
    {
        if (searchLeaders.ContainsKey(leader.getId()))
        {
            return;
        }

        searchSettings.AddOrUpdate(leader.getId(), settings);
        searchLeaders.AddOrUpdate(leader.getId(), leader);
        addQueueLeader(leader);
    }

    public void unregisterPartyLeader(IPlayer leader)
    {
        if (searchLeaders.Remove(leader.getId(), out var toRemove) && toRemove != null)
        {
            removeQueueLeader(toRemove);
            searchSettings.Remove(leader.getId());
        }
        else
        {
            unregisterLongTermPartyLeader(leader);
        }
    }

    private IPlayer? searchPlayer(IPlayer leader)
    {
        var settings = searchSettings.GetValueOrDefault(leader.getId());
        if (settings != null)
        {
            int minLevel = settings.minLevel, maxLevel = settings.maxLevel;
            Collections.shuffle(settings.searchedJobs);

            int leaderCid = leader.getId();
            int leaderMapid = leader.getMapId();
            foreach (Job searchJob in settings.searchedJobs)
            {
                var chr = fetchPlayer(leaderCid, leaderMapid, searchJob, minLevel, maxLevel);
                if (chr != null)
                {
                    return chr;
                }
            }
        }

        return null;
    }

    private bool sendPartyInviteFromSearch(IPlayer chr, IPlayer leader)
    {
        if (chr == null)
        {
            return false;
        }

        int partyid = leader.getPartyId();
        if (partyid < 0)
        {
            return false;
        }

        if (InviteCoordinator.createInvite(InviteType.PARTY, leader, partyid, chr.getId()))
        {
            chr.disablePartySearchInvite(leader.getId());
            chr.sendPacket(PacketCreator.partySearchInvite(leader));
            return true;
        }
        else
        {
            return false;
        }
    }

    private KeyValuePair<List<IPlayer>, List<IPlayer>> fetchQueuedLeaders()
    {
        List<IPlayer> queuedLeaders, nextLeaders;

        leaderQueueLock.EnterWriteLock();
        try
        {
            int SplitIdx = Math.Min(leaderQueue.Count, 100);

            queuedLeaders = new(leaderQueue.Take(SplitIdx));
            nextLeaders = new(leaderQueue.Skip(SplitIdx).Take(leaderQueue.Count));
        }
        finally
        {
            leaderQueueLock.ExitWriteLock();
        }

        return new(queuedLeaders, nextLeaders);
    }

    private void registerLongTermPartyLeaders(List<KeyValuePair<IPlayer, LeaderSearchMetadata>> recycledLeaders)
    {
        leaderQueueLock.EnterReadLock();
        try
        {
            foreach (var p in recycledLeaders)
            {
                timeoutLeaders.AddOrUpdate(p.Key, p.Value);
            }
        }
        finally
        {
            leaderQueueLock.ExitReadLock();
        }
    }

    private void unregisterLongTermPartyLeader(IPlayer leader)
    {
        leaderQueueLock.EnterReadLock();
        try
        {
            timeoutLeaders.Remove(leader);
        }
        finally
        {
            leaderQueueLock.ExitReadLock();
        }
    }

    private void reinstateLongTermPartyLeaders()
    {
        Dictionary<IPlayer, LeaderSearchMetadata> timeoutLeadersCopy;
        leaderQueueLock.EnterWriteLock();
        try
        {
            timeoutLeadersCopy = new(timeoutLeaders);
            timeoutLeaders.Clear();
        }
        finally
        {
            leaderQueueLock.ExitWriteLock();
        }

        foreach (var e in timeoutLeadersCopy)
        {
            registerPartyLeader(e.Key, e.Value);
        }
    }

    public void runPartySearch()
    {
        var queuedLeaders = fetchQueuedLeaders();

        List<IPlayer> searchedLeaders = new();
        List<IPlayer> recalledLeaders = new();
        List<IPlayer> expiredLeaders = new();

        foreach (IPlayer leader in queuedLeaders.Key)
        {
            var chr = searchPlayer(leader);
            if (sendPartyInviteFromSearch(chr, leader))
            {
                searchedLeaders.Add(leader);
            }
            else
            {
                LeaderSearchMetadata? settings = searchSettings.GetValueOrDefault(leader.getId());
                if (settings != null)
                {
                    if (settings.reentryCount < YamlConfig.config.server.PARTY_SEARCH_REENTRY_LIMIT)
                    {
                        settings.reentryCount += 1;
                        recalledLeaders.Add(leader);
                    }
                    else
                    {
                        expiredLeaders.Add(leader);
                    }
                }
            }
        }

        leaderQueueLock.EnterReadLock();
        try
        {
            leaderQueue.Clear();
            leaderQueue.AddRange(queuedLeaders.Value);

            try
            {
                leaderQueue.InsertRange(25, recalledLeaders);
            }
            catch (ArgumentOutOfRangeException)
            {
                leaderQueue.AddRange(recalledLeaders);
            }
        }
        finally
        {
            leaderQueueLock.ExitReadLock();
        }

        foreach (IPlayer leader in searchedLeaders)
        {
            var party = leader.getParty();
            if (party != null && party.getMembers().Count < 6)
            {
                addQueueLeader(leader);
            }
            else
            {
                if (leader.isLoggedinWorld())
                {
                    leader.dropMessage(5, "Your Party Search token session has finished as your party reached full capacity.");
                }
                searchLeaders.Remove(leader.getId());
                searchSettings.Remove(leader.getId());
            }
        }

        List<KeyValuePair<IPlayer, LeaderSearchMetadata>> recycledLeaders = new();
        foreach (IPlayer leader in expiredLeaders)
        {
            searchLeaders.Remove(leader.getId());
            searchSettings.Remove(leader.getId(), out var settings);

            if (leader.isLoggedinWorld())
            {
                if (settings != null)
                {
                    recycledLeaders.Add(new(leader, settings));
                    if (YamlConfig.config.server.USE_DEBUG && leader.isGM())
                    {
                        leader.dropMessage(5, "Your Party Search token session is now on waiting queue for up to 7 minutes, to get it working right away please stop your Party Search and retry again later.");
                    }
                }
                else
                {
                    leader.dropMessage(5, "Your Party Search token session expired, please stop your Party Search and retry again later.");
                }
            }
        }

        if (recycledLeaders.Count > 0)
        {
            registerLongTermPartyLeaders(recycledLeaders);
        }

        updateCount++;
        if (updateCount % 77 == 0)
        {
            reinstateLongTermPartyLeaders();
        }
    }

}

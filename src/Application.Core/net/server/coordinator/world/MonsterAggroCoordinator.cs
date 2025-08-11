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


using Application.Core.Channel;
using Application.Core.Game.Life;

namespace net.server.coordinator.world;



/**
 * @author Ronan
 */
public class MonsterAggroCoordinator
{
    readonly WorldChannel _channelServer;
    private object lockObj = new object();
    private object idleLock = new object();
    private long lastStopTime;

    private ScheduledFuture? aggroMonitor = null;

    private Dictionary<Monster, Dictionary<int, PlayerAggroEntry>> mobAggroEntries = new();
    private Dictionary<Monster, List<PlayerAggroEntry>> mobSortedAggros = new();

    private HashSet<int> mapPuppetEntries = new();

    public MonsterAggroCoordinator(WorldChannel channelServer)
    {
        _channelServer = channelServer;
        lastStopTime = _channelServer.Container.getCurrentTime();
    }

    private class PlayerAggroEntry(int cid)
    {
        public int cid = cid;
        public int averageDamage = 0;
        public int currentDamageInstances = 0;
        public long accumulatedDamage = 0;

        public int expireStreak = 0;
        public int updateStreak = 0;
        public int toNextUpdate = 0;
        public int entryRank = -1;
    }

    public void stopAggroCoordinator()
    {
        Monitor.Enter(idleLock);
        try
        {
            if (aggroMonitor == null)
            {
                return;
            }

            aggroMonitor.cancel(false);
            aggroMonitor = null;
        }
        finally
        {
            Monitor.Exit(idleLock);
        }

        lastStopTime = _channelServer.Container.getCurrentTime();
    }

    public void startAggroCoordinator()
    {
        Monitor.Enter(idleLock);
        try
        {
            if (aggroMonitor != null)
            {
                return;
            }

            aggroMonitor = _channelServer.Container.TimerManager.register(() =>
            {
                runAggroUpdate(1);
                runSortLeadingCharactersAggro();
            }, YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL, YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL);
        }
        finally
        {
            Monitor.Exit(idleLock);
        }

        int timeDelta = (int)Math.Ceiling((double)(_channelServer.Container.getCurrentTime() - lastStopTime) / YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL);
        if (timeDelta > 0)
        {
            runAggroUpdate(timeDelta);
        }
    }

    private static void updateEntryExpiration(PlayerAggroEntry pae)
    {
        pae.toNextUpdate = (int)Math.Ceiling((120000L / YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL) / Math.Pow(2, pae.expireStreak + pae.currentDamageInstances));
    }

    private static void insertEntryDamage(PlayerAggroEntry pae, int damage)
    {
        lock (pae)
        {
            long totalDamage = pae.averageDamage;
            totalDamage *= pae.currentDamageInstances;
            totalDamage += damage;

            pae.expireStreak = 0;
            pae.updateStreak = 0;
            updateEntryExpiration(pae);

            pae.currentDamageInstances += 1;
            pae.averageDamage = (int)(totalDamage / pae.currentDamageInstances);
            pae.accumulatedDamage = totalDamage;
        }
    }

    private static bool expiredAfterUpdateEntryDamage(PlayerAggroEntry pae, int deltaTime)
    {
        lock (pae)
        {
            pae.updateStreak += 1;
            pae.toNextUpdate -= deltaTime;

            if (pae.toNextUpdate <= 0)
            {    // reached dmg instance expire time
                pae.expireStreak += 1;
                updateEntryExpiration(pae);

                pae.currentDamageInstances -= 1;
                if (pae.currentDamageInstances < 1)
                {   // expired aggro for this player
                    return true;
                }
                pae.accumulatedDamage = pae.averageDamage * pae.currentDamageInstances;
            }

            return false;
        }
    }

    public void addAggroDamage(Monster mob, int cid, int damage)
    { // assumption: should not trigger after dispose()
        if (!mob.isAlive())
        {
            return;
        }

        var sortedAggro = mobSortedAggros.GetValueOrDefault(mob);
        Dictionary<int, PlayerAggroEntry>? mobAggro = mobAggroEntries.GetValueOrDefault(mob);
        if (mobAggro == null)
        {
            if (Monitor.TryEnter(lockObj))
            {   // can run unreliably, as fast as possible... try lock that is!
                try
                {
                    mobAggro = mobAggroEntries.GetValueOrDefault(mob);
                    if (mobAggro == null)
                    {
                        mobAggro = new();
                        mobAggroEntries.AddOrUpdate(mob, mobAggro);

                        sortedAggro = new();
                        mobSortedAggros.AddOrUpdate(mob, sortedAggro);
                    }
                    else
                    {
                        sortedAggro = mobSortedAggros.GetValueOrDefault(mob);
                    }
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                return;
            }
        }

        var aggroEntry = mobAggro.GetValueOrDefault(cid);
        if (aggroEntry == null)
        {
            aggroEntry = new PlayerAggroEntry(cid);

            lock (mobAggro)
            {
                lock (sortedAggro!)
                {
                    var mappedEntry = mobAggro.GetValueOrDefault(cid);

                    if (mappedEntry == null)
                    {
                        mobAggro.AddOrUpdate(aggroEntry.cid, aggroEntry);
                        sortedAggro.Add(aggroEntry);
                    }
                    else
                    {
                        aggroEntry = mappedEntry;
                    }
                }
            }
        }
        else if (damage < 1)
        {
            return;
        }

        insertEntryDamage(aggroEntry, damage);
    }

    private void runAggroUpdate(int deltaTime)
    {
        List<KeyValuePair<Monster, Dictionary<int, PlayerAggroEntry>>> aggroMobs = new();
        Monitor.Enter(lockObj);
        try
        {
            aggroMobs = mobAggroEntries.ToList();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        foreach (var am in aggroMobs)
        {
            Dictionary<int, PlayerAggroEntry> mobAggro = am.Value;
            var sortedAggro = mobSortedAggros.GetValueOrDefault(am.Key);

            if (sortedAggro != null)
            {
                List<int> toRemove = new();
                List<int> toRemoveIdx = new(mobAggro.Count);
                List<int> toRemoveByFetch = new();

                lock (mobAggro)
                {
                    lock (sortedAggro)
                    {
                        foreach (PlayerAggroEntry pae in mobAggro.Values)
                        {
                            if (expiredAfterUpdateEntryDamage(pae, deltaTime))
                            {
                                toRemove.Add(pae.cid);
                                if (pae.entryRank > -1)
                                {
                                    toRemoveIdx.Add(pae.entryRank);
                                }
                                else
                                {
                                    toRemoveByFetch.Add(pae.cid);
                                }
                            }
                        }

                        if (toRemove.Count > 0)
                        {
                            foreach (int cid in toRemove)
                            {
                                mobAggro.Remove(cid);
                            }

                            if (mobAggro.Count == 0)
                            {   // all aggro on this mob expired
                                if (!am.Key.isBoss())
                                {
                                    am.Key.aggroResetAggro();
                                }
                            }
                        }

                        if (toRemoveIdx.Count > 0)
                        {
                            // last to first indexes
                            toRemoveIdx.Sort((p1, p2) => p1 < p2 ? 1 : p1.Equals(p2) ? 0 : -1);

                            foreach (int idx in toRemoveIdx)
                            {
                                sortedAggro.RemoveAt(idx);
                            }
                        }

                        if (toRemoveByFetch.Count > 0)
                        {
                            foreach (int cid in toRemoveByFetch)
                            {
                                for (int i = 0; i < sortedAggro.Count; i++)
                                {
                                    if (cid.Equals(sortedAggro.get(i).cid))
                                    {
                                        sortedAggro.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void insertionSortAggroList(List<PlayerAggroEntry> paeList)
    {
        for (int i = 1; i < paeList.Count; i++)
        {
            PlayerAggroEntry pae = paeList.get(i);
            long curAccDmg = pae.accumulatedDamage;

            int j = i - 1;
            while (j >= 0 && curAccDmg > paeList.get(j).accumulatedDamage)
            {
                j -= 1;
            }

            j += 1;
            if (j != i)
            {
                paeList.RemoveAt(i);
                paeList.Insert(j, pae);
            }
        }

        int d = 0;
        foreach (PlayerAggroEntry pae in paeList)
        {
            pae.entryRank = d;
            d += 1;
        }
    }

    public bool isLeadingCharacterAggro(Monster mob, IPlayer player)
    {
        if (mob.isLeadingPuppetInVicinity())
        {
            return false;
        }
        else if (mob.isCharacterPuppetInVicinity(player))
        {
            return true;
        }

        // by assuming the quasi-sorted nature of "mobAggroList", this method
        // returns whether the player given as parameter can be elected as next aggro leader

        var mobAggroList = mobSortedAggros.GetValueOrDefault(mob);
        if (mobAggroList != null)
        {

            mobAggroList = new(mobAggroList.Take(Math.Min(mobAggroList.Count, 5)));

            var map = mob.getMap();
            foreach (PlayerAggroEntry pae in mobAggroList)
            {
                var chr = map.getCharacterById(pae.cid);
                if (chr != null)
                {
                    if (player.getId() == pae.cid)
                    {
                        return true;
                    }
                    else if (pae.updateStreak < YamlConfig.config.server.MOB_STATUS_AGGRO_PERSISTENCE && chr.isAlive())
                    {  // verifies currently leading players activity
                        return false;
                    }
                }
            }
        }

        return false;
    }

    public void runSortLeadingCharactersAggro()
    {
        List<List<PlayerAggroEntry>> aggroList;
        Monitor.Enter(lockObj);
        try
        {
            aggroList = new(mobSortedAggros.Values);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        foreach (List<PlayerAggroEntry> mobAggroList in aggroList)
        {
            lock (mobAggroList)
            {
                insertionSortAggroList(mobAggroList);
            }
        }
    }

    public void removeAggroEntries(Monster mob)
    {
        Monitor.Enter(lockObj);
        try
        {
            mobAggroEntries.Remove(mob);
            mobSortedAggros.Remove(mob);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void addPuppetAggro(IPlayer player)
    {
        lock (mapPuppetEntries)
        {
            mapPuppetEntries.Add(player.getId());
        }
    }

    public void removePuppetAggro(int cid)
    {
        lock (mapPuppetEntries)
        {
            mapPuppetEntries.Remove(cid);
        }
    }

    public List<int> getPuppetAggroList()
    {
        lock (mapPuppetEntries)
        {
            return new(mapPuppetEntries);
        }
    }

    public void dispose()
    {
        stopAggroCoordinator();

        Monitor.Enter(lockObj);
        try
        {
            mobAggroEntries.Clear();
            mobSortedAggros.Clear();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }
}

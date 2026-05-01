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


using Application.Core.Channel.Tasks;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Utility.Tickables;

namespace net.server.coordinator.world;



/**
 * @author Ronan
 */
public class MonsterAggroCoordinator : ILoopTickable
{
    private Dictionary<Monster, PlayerAggroObject> mobAggroEntries = new();

    private HashSet<int> mapPuppetEntries = new();
    MapleMap Map { get; }

    public long Period { get; private set; }

    public long Next { get; private set; }

    public TickableStatus Status { get; private set; }

    public MonsterAggroCoordinator(MapleMap map)
    {
        Map = map;

        var now = map.ChannelServer.Node.getCurrentTime(); ;
        Period = YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL;
        Next = now + Period;
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

    private record PlayerAggroObject(Dictionary<int, PlayerAggroEntry> Items, List<PlayerAggroEntry> CachedView);

    public void stopAggroCoordinator()
    {
        Status = TickableStatus.InActive;
    }

    public void RunAggro()
    {
        runAggroUpdate(1);
        runSortLeadingCharactersAggro();
    }

    private static void updateEntryExpiration(PlayerAggroEntry pae)
    {
        pae.toNextUpdate = (int)Math.Ceiling((120000L / YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL) / Math.Pow(2, pae.expireStreak + pae.currentDamageInstances));
    }

    private static void insertEntryDamage(PlayerAggroEntry pae, int damage)
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

    private static bool expiredAfterUpdateEntryDamage(PlayerAggroEntry pae, int deltaTime)
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

    public void addAggroDamage(Monster mob, int cid, int damage)
    { // assumption: should not trigger after dispose()
        if (!mob.isAlive())
        {
            return;
        }

        var mobAggro = mobAggroEntries.GetValueOrDefault(mob);
        if (mobAggro == null)
        {
            mobAggro = new(new Dictionary<int, PlayerAggroEntry>(), new List<PlayerAggroEntry>());
            mobAggroEntries[mob] = mobAggro;
        }

        var aggroEntry = mobAggro.Items.GetValueOrDefault(cid);
        if (aggroEntry == null)
        {
            aggroEntry = new PlayerAggroEntry(cid);

            mobAggro.Items[cid] = aggroEntry;
            mobAggro.CachedView.Add(aggroEntry);
        }
        else if (damage < 1)
        {
            return;
        }

        insertEntryDamage(aggroEntry, damage);
    }

    private void runAggroUpdate(int deltaTime)
    {
        var aggroMobs = mobAggroEntries.ToList();

        foreach (var am in aggroMobs)
        {
            var dataList = am.Value.CachedView.ToArray();

            List<int> toRemove = new();

            foreach (PlayerAggroEntry pae in dataList)
            {
                if (expiredAfterUpdateEntryDamage(pae, deltaTime))
                {
                    toRemove.Add(pae.cid);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (int cid in toRemove)
                {
                    am.Value.Items.Remove(cid);
                }

                if (am.Value.Items.Count == 0)
                {   // all aggro on this mob expired
                    if (!am.Key.isBoss())
                    {
                        am.Key.aggroResetAggro();
                    }
                }
            }

            am.Value.CachedView.RemoveAll(x => toRemove.Contains(x.cid));

        }
    }


    public bool isLeadingCharacterAggro(Monster mob, Player player)
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

        var obj = mobAggroEntries.GetValueOrDefault(mob);
        if (obj != null)
        {
            var mobAggroList = obj.CachedView.Take(5).ToArray();

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
        var aggroList = mobAggroEntries.Values.ToArray();

        foreach (var mobAggroList in aggroList)
        {
            mobAggroList.CachedView.Sort((a, b) => b.accumulatedDamage.CompareTo(a.accumulatedDamage));

            for (int i = 0; i < mobAggroList.CachedView.Count; i++)
            {
                mobAggroList.CachedView[i].entryRank = i;
            }
        }
    }

    public void removeAggroEntries(Monster mob)
    {
        mobAggroEntries.Remove(mob);
    }

    public void addPuppetAggro(Player player)
    {
        mapPuppetEntries.Add(player.getId());
    }

    public void removePuppetAggro(int cid)
    {
        mapPuppetEntries.Remove(cid);
    }

    public List<int> getPuppetAggroList()
    {
        return new(mapPuppetEntries);
    }

    public void dispose()
    {
        Status = TickableStatus.Remove;
        stopAggroCoordinator();

        mobAggroEntries.Clear();
    }

    public void OnTick(long now)
    {
        if (!this.IsAvailable())
        {
            return;
        }

        if (Next <= now)
        {
            int timeDelta = (int)Math.Ceiling((double)(now - Next) / YamlConfig.config.server.MOB_STATUS_AGGRO_INTERVAL) + 1;
            runAggroUpdate(timeDelta);
            runSortLeadingCharactersAggro();

            Next = now + Period;
        }
    }
}

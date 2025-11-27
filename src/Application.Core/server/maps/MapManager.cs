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
using Application.Core.Channel.Performance;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace server.maps;

public class MapManager : IDisposable
{
    private AbstractEventInstanceManager? evt;
    readonly WorldChannel _channelServer;

    private ConcurrentDictionary<int, IMap> maps = new();

    public MapManager(AbstractEventInstanceManager? eim, WorldChannel worldChannel)
    {
        _channelServer = worldChannel;
        this.evt = eim;
    }

    public IMap resetMap(int mapid, out IMap? oldMap)
    {
        if (maps.TryRemove(mapid, out oldMap))
            _channelServer.Metrics.ActiveMaps.Dec();
        return getMap(mapid);
    }

    object loadWZLock = new object();
    private IMap loadMapFromWz(int mapid, bool cache)
    {
        lock (loadWZLock)
        {
            IMap? map;

            if (cache)
            {
                map = maps.GetValueOrDefault(mapid);

                if (map != null)
                {
                    return map;
                }
            }

            map = MapFactory.Instance.loadMapFromWz(mapid, _channelServer, evt);
            _channelServer.Metrics.ActiveMaps.Inc();
            if (cache)
            {
                maps.AddOrUpdate(mapid, map);
            }

            return map;
        }

    }

    public IMap getMap(int mapid)
    {
        return loadMapFromWz(mapid, true);
    }

    public IMap getDisposableMap(int mapid)
    {
        return loadMapFromWz(mapid, false);
    }

    public bool isMapLoaded(int mapId)
    {
        return maps.ContainsKey(mapId);
    }

    public bool TryGetMap(int mapId, out IMap map)
    {
        return maps.TryGetValue(mapId, out map);
    }

    public Dictionary<int, IMap> getMaps()
    {
        return new(maps);
    }

    public void updateMaps()
    {
        var sw = Stopwatch.StartNew();
        foreach (IMap map in getMaps().Values)
        {
            map.respawn();
            map.mobMpRecovery();
        }
        sw.Stop();
        _channelServer.Metrics.MapTick(sw.Elapsed.TotalMilliseconds);
    }

    bool disposed = false;
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        foreach (var kv in getMaps())
        {
            if (maps.TryRemove(kv.Key, out var v))
            {
                v.Dispose();
                _channelServer.Metrics.ActiveMaps.Dec();
            }
        }

        this.evt = null;
    }

    public void CheckActive()
    {
        foreach (var map in getMaps())
        {
            if (!map.Value.IsTrackedByEvent && map.Value.EventInstanceManager == null && !map.Value.IsActive())
            {
                map.Value.Dispose();
                maps.TryRemove(map.Key, out _);
                _channelServer.Metrics.ActiveMaps.Dec();
            }
        }
    }
}

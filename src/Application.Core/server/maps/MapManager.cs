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
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace server.maps;

public class MapManager : IDisposable, INamedInstance, ITickable
{
    public string InstanceName { get; }
    public TickableStatus Status { get; private set; }

    private AbstractEventInstanceManager? evt;
    readonly WorldChannel _channelServer;

    private ConcurrentDictionary<int, IMap> maps = new();

    public MapManager(AbstractEventInstanceManager? eim, WorldChannel worldChannel)
    {
        _channelServer = worldChannel;
        this.evt = eim;

        InstanceName = $"{worldChannel.InstanceName}:{nameof(MapManager)}:{(evt == null ? "None" : evt.getName())}";
    }

    public IMap resetMap(int mapid, out IMap? oldMap)
    {
        RemoveMap(mapid, out oldMap);
        return getMap(mapid);
    }

    private IMap loadMapFromWz(int mapid)
    {
        if (maps.TryGetValue(mapid, out var map) && map != null)
        {
            return map;
        }


        map = MapFactory.Instance.loadMapFromWz(mapid, _channelServer, evt);
        maps[mapid] = map;

        GameMetrics.ActiveMapCount.Add(1, 
            new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName), 
            new KeyValuePair<string, object?>("Name", InstanceName));
        return map;
    }

    public IMap getMap(int mapid)
    {
        return loadMapFromWz(mapid);
    }


    public bool isMapLoaded(int mapId)
    {
        return maps.ContainsKey(mapId);
    }

    public bool TryGetMap(int mapId, out IMap? map)
    {
        return maps.TryGetValue(mapId, out map);
    }

    public Dictionary<int, IMap> getMaps()
    {
        return new(maps);
    }

    public List<IMap> GetAllMaps() => maps.Values.ToList();

    public void OnTick(long now)
    {
        var sw = Stopwatch.StartNew();

        foreach (var item in maps.Values.ToList())
        {
            if (item is ITickable tickable)
            {
                tickable.OnTick(now);

                if (tickable.Status == TickableStatus.Remove)
                {
                    RemoveMap(item.Id, out _);
                }
            }
        }

        sw.Stop();

        GameMetrics.MapTickDuration.Record(sw.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName),
            new KeyValuePair<string, object?>("Name", InstanceName));
    }

    void RemoveMap(int mapId, out IMap? oldMap)
    {
        if (maps.TryRemove(mapId, out oldMap))
        {
            GameMetrics.ActiveMapCount.Add(-1
                , new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName)
                , new KeyValuePair<string, object?>("Name", InstanceName));
            oldMap.Dispose();
        }
    }


    public void Dispose()
    {
        if (!this.IsAvailable())
            return;

        Status = TickableStatus.Remove;
        foreach (var kv in getMaps())
        {
            RemoveMap(kv.Key, out _);
        }

        this.evt = null;
    }
}

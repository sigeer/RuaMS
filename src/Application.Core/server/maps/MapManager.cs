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
using Application.Core.scripting.Events.Instances;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace server.maps;

public class MapManager : IAsyncDisposable, INamedInstance, ITickable
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

    public async Task<IMap> resetMap(int mapid)
    {
        await RemoveMap(mapid);
        return await getMap(mapid);
    }

    private async Task<IMap> loadMapFromWz(int mapid)
    {
        if (maps.TryGetValue(mapid, out var map) && map != null)
        {
            return map;
        }


        map = await MapFactory.Instance.loadMapFromWz(mapid, _channelServer, evt);
        maps[mapid] = map;
        await _channelServer.NodeService.PluginManager.OnMapLoad(map);

        GameMetrics.ActiveMapCount.Add(1,
            new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName),
            new KeyValuePair<string, object?>("Name", InstanceName));
        return map;
    }

    public async Task<IMap> getMap(int mapid)
    {
        return await loadMapFromWz(mapid);
    }


    public bool isMapLoaded(int mapId)
    {
        return maps.ContainsKey(mapId);
    }

    public bool TryGetMap(int mapId, [MaybeNullWhen(false)] out IMap map)
    {
        return maps.TryGetValue(mapId, out map);
    }

    public Dictionary<int, IMap> getMaps()
    {
        return new(maps);
    }

    public List<IMap> GetAllMaps() => maps.Values.ToList();

    public async Task OnTick(long now)
    {
        var sw = Stopwatch.StartNew();

        foreach (var item in maps.Values.ToList())
        {
            if (item is ITickable tickable)
            {
                await item.OnTick(now);

                if (tickable.Status == TickableStatus.Remove)
                {
                    await RemoveMap(item.Id);
                }
            }
        }

        sw.Stop();

        GameMetrics.MapTickDuration.Record(sw.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName),
            new KeyValuePair<string, object?>("Name", InstanceName));
    }

    async Task RemoveMap(int mapId)
    {
        if (maps.TryRemove(mapId, out var oldMap))
        {
            GameMetrics.ActiveMapCount.Add(-1
                , new KeyValuePair<string, object?>("Channel", _channelServer.InstanceName)
                , new KeyValuePair<string, object?>("Name", InstanceName));
            await _channelServer.NodeService.PluginManager.OnMapUnload(oldMap);
            await oldMap.DisposeAsync();
        }
    }


    public async ValueTask DisposeAsync()
    {
        if (!this.IsAvailable())
            return;

        Status = TickableStatus.Remove;
        foreach (var kv in getMaps())
        {
            await RemoveMap(kv.Key);
        }

        this.evt = null;
    }
}
